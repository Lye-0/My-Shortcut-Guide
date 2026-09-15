using MyShortcutGuide.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetDefaultFont(Theme.Font());
        Application.SetColorMode(SystemColorMode.Dark);
        using var dialog = new CaptureDialog();
        dialog.ShowInTaskbar = true;
        var result = dialog.ShowDialog();
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "capture-result.txt"), $"{result}: {dialog.Result?.Gesture}");
    }
}
