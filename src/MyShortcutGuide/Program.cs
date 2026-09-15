using System.Runtime.InteropServices;
using System.Security.Principal;

namespace MyShortcutGuide;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetDefaultFont(new Font("Yu Gothic UI", 10));
        Application.SetColorMode(SystemColorMode.Dark);
        var identity = WindowsIdentity.GetCurrent().User!.Value;
        using var activation = new EventWaitHandle(false, EventResetMode.AutoReset, $@"Local\MyShortcutGuide.Activate.{identity}");
        using var mutex = new Mutex(true, $@"Local\MyShortcutGuide.Instance.{identity}", out var first);
        if (!first)
        {
            AllowSetForegroundWindow(-1);
            activation.Set();
            return;
        }
        try
        {
            using var context = new TrayContext(activation, args.Contains("--background", StringComparer.OrdinalIgnoreCase));
            Application.Run(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"起動できませんでした。既存のデータは保持されています。\n\n{ex.Message}", "My Shortcut Guide", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { mutex.ReleaseMutex(); }
    }
    [DllImport("user32.dll")] private static extern bool AllowSetForegroundWindow(int processId);
}
