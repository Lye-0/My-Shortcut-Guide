using System.Diagnostics;
using Microsoft.Win32;
using MyShortcutGuide.Core;

namespace MyShortcutGuide.Services;

internal static class AppPaths
{
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyShortcutGuide");
    public static string Json => Path.Combine(DataDirectory, "shortcuts.json");
    public static string Manifests => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "KeyboardShortcuts");
}

internal sealed class StartupService(string? executablePath = null)
{
    private const string RunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string Name = "MyShortcutGuide";
    public string? GetCommand() { using var key = Registry.CurrentUser.OpenSubKey(RunPath); return key?.GetValue(Name) as string; }
    public bool IsEnabled => GetCommand() is not null;
    public void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunPath, true);
        if (enabled)
        {
            var path = executablePath ?? Environment.ProcessPath ?? throw new InvalidOperationException("EXEの場所を取得できません。");
            if (!string.Equals(Path.GetFileName(path), "MyShortcutGuide.exe", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("自動起動はMyShortcutGuide.exeから設定してください。");
            key.SetValue(Name, $"\"{path}\" --background");
        }
        else key.DeleteValue(Name, false);
    }
    public void Restore(string? value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunPath, true);
        if (value is null) key.DeleteValue(Name, false); else key.SetValue(Name, value);
    }
}

internal sealed class PowerToysService
{
    // Internal PowerToys adapter; keep this version-sensitive detail isolated.
    private const string Trigger = @"Local\ShortcutGuide-TriggerEvent-d4275ad3-2531-4d19-9252-c0becbd9b496";
    public static string? FindGenerator()
    {
        foreach (var process in Process.GetProcessesByName("PowerToys.ShortcutGuide"))
        {
            using (process)
            {
                try
                {
                    var path = Path.Combine(Path.GetDirectoryName(process.MainModule!.FileName)!, "PowerToys.ShortcutGuide.IndexYmlGenerator.exe");
                    if (File.Exists(path)) return path;
                }
                catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException) { }
            }
        }
        foreach (var root in new[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PowerToys"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerToys") })
        {
            var path = Path.Combine(root, "WinUI3Apps", "PowerToys.ShortcutGuide.IndexYmlGenerator.exe");
            if (File.Exists(path)) return path;
        }
        return null;
    }
    public async Task<string?> RefreshAsync()
    {
        var generator = FindGenerator();
        if (generator is null) return "YAMLを保存しました。PowerToysを起動し、Shortcut Guideを有効にしてください。";
        var indexPath = Path.Combine(AppPaths.Manifests, "index.yml");
        var previousIndex = File.Exists(indexPath) ? File.ReadAllText(indexPath) : null;
        if (previousIndex is not null) AtomicFile.Write(Path.Combine(AppPaths.DataDirectory, "powertoys-index.last-good.yml.bak"), previousIndex);
        using var process = Process.Start(new ProcessStartInfo(generator) { UseShellExecute = false, CreateNoWindow = true });
        if (process is null) return "索引を更新できません。PowerToysを再起動してください。";
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill();
            await process.WaitForExitAsync();
            RestoreMissingIndex();
            return "PowerToysの索引更新が時間内に完了しませんでした。「ガイドへ再反映」で再試行できます。";
        }
        if (process.ExitCode != 0) RestoreMissingIndex();
        return process.ExitCode == 0 ? null : "PowerToysの索引更新に失敗しました。YAMLは保存済みです。PowerToysを再起動してください。";

        void RestoreMissingIndex()
        {
            // The PowerToys generator deletes index.yml before parsing. Restore only when absent;
            // never overwrite an index another process may have regenerated concurrently.
            if (previousIndex is not null && !File.Exists(indexPath)) AtomicFile.Write(indexPath, previousIndex);
        }
    }
    public void Open()
    {
        var running = Process.GetProcessesByName("PowerToys");
        var isRunning = running.Length > 0;
        foreach (var process in running) process.Dispose();
        if (!isRunning)
            throw new InvalidOperationException("PowerToysを起動し、Shortcut Guideを有効にしてください。");
        if (!EventWaitHandle.TryOpenExisting(Trigger, out var handle))
            throw new InvalidOperationException("Shortcut Guideを開けません。PowerToysでShortcut Guideを有効にしてください。対応しないバージョンではPowerToys側の起動ショートカットを使用できます。");
        using (handle) handle.Set();
    }
}

internal sealed class AppController
{
    private readonly DocumentStore store = new(AppPaths.Json);
    public StartupService Startup { get; } = new();
    public PowerToysService PowerToys { get; } = new();
    public ShortcutDocument Document { get; private set; }
    public string Status { get; private set; } = "準備中";
    public bool IsBusy { get; private set; }
    public bool HasWarning { get; private set; }
    public event EventHandler? Changed;
    public AppController() => Document = store.Load(); // Fail closed: never replace an unreadable JSON with defaults.
    public async Task InitializeAsync()
    {
        Document.Settings.StartWithWindows = Startup.IsEnabled;
        if (!File.Exists(AppPaths.Json)) store.Save(Document);
        await RegenerateAsync();
    }
    public async Task<bool> SaveAsync(ShortcutDocument next, bool applyStartup = false)
    {
        if (IsBusy) return false;
        next.Validate();
        var previousCommand = Startup.GetCommand();
        try
        {
            if (applyStartup) Startup.Set(next.Settings.StartWithWindows);
            store.Save(next);
        }
        catch
        {
            if (applyStartup) Startup.Restore(previousCommand);
            throw;
        }
        Document = next;
        await RegenerateAsync();
        return true;
    }
    public async Task RegenerateAsync()
    {
        if (IsBusy) return;
        IsBusy = true; Status = "保存内容をShortcut Guideへ反映中…"; HasWarning = false; Changed?.Invoke(this, EventArgs.Empty);
        try
        {
            ManifestWriter.Write(Document, AppPaths.Manifests);
            var warning = await PowerToys.RefreshAsync();
            HasWarning = warning is not null;
            Status = warning ?? (Document.Settings.ShowInShortcutGuide ? "保存済み · ガイドを開き直すと反映されます" : "保存済み · Shortcut Guideへの表示はオフ");
        }
        catch (Exception ex)
        {
            HasWarning = true;
            Status = $"JSONは保持されています。ガイドへの反映を再試行してください: {ex.Message}";
        }
        finally { IsBusy = false; Changed?.Invoke(this, EventArgs.Empty); }
    }
}
