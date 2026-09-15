using MyShortcutGuide.Core;
using MyShortcutGuide.Services;
using MyShortcutGuide.UI;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        Application.SetDefaultFont(Theme.Font());
        Application.SetColorMode(SystemColorMode.Dark);
        try
        {
            if (args.Length == 2 && args[0] == "--capture")
            {
                using var capture = new CaptureDialog();
                capture.ShowDialog();
                if (capture.Result is null) return 2;
                File.WriteAllText(args[1], capture.Result.Gesture);
                return 0;
            }
            var sections = ShortcutDocument.CreateDefault().Sections;
            var entry = new ShortcutEntry();
            using (var dialog = new ShortcutDialog(sections, sections[0].Id, entry, false))
            {
                dialog.Shown += (_, _) => dialog.BeginInvoke(() =>
                {
                    var controls = All(dialog).ToArray();
                    var name = controls.OfType<TextBox>().Single(c => c.AccessibleName == "名前");
                    name.Text = "検索: 日本語";
                    controls.OfType<TextBox>().Single(c => c.AccessibleName == "説明").Text = "line1\nline2";
                    controls.OfType<CheckBox>().Single(c => c.Text == "Ctrl").Checked = true;
                    controls.OfType<ComboBox>().Single(c => c.AccessibleName == "メインキー").SelectedItem = KeyCatalog.All.Single(k => k.Code == 70);
                    controls.OfType<ComboBox>().Single(c => c.AccessibleName == "セクション").SelectedItem = sections[2];
                    controls.OfType<CheckBox>().Single(c => c.Text.Contains("Recommended")).Checked = true;
                    foreach (var label in controls.OfType<Label>().Where(c => c.AutoSize && c.Visible))
                        if (label.Height < label.PreferredHeight) throw new Exception($"Clipped label: {label.Text}: {label.Height}/{label.PreferredHeight}");
                    controls.OfType<Button>().Single(c => c.Text == "保存").PerformClick();
                });
                var result = dialog.ShowDialog();
                Check(result == DialogResult.OK && entry.Ctrl && entry.KeyCode == 70 && entry.Name == "検索: 日本語" && entry.Recommended && dialog.SectionId == sections[2].Id);
                Console.WriteLine("PASS real shortcut form save, modifiers, main key, section and labels");
            }
            using (var editor = new EditorForm(new AppController(), () => { }, () => { }))
            {
                editor.Shown += (_, _) => editor.BeginInvoke(() =>
                {
                    var controls = All(editor).ToArray();
                    var input = controls.OfType<TextBox>().Single(c => c.AccessibleName == "選択セクション内を検索");
                    Check(input.Parent is InputFrame && input.Left > 0 && input.Top > 0);
                    Check(input.Bottom < input.Parent!.ClientSize.Height);
                    var actions = controls.Single(c => c.Name == "AppActions");
                    Check(actions.Parent!.Name == "Sidebar");
                    foreach (var button in All(actions).OfType<Button>())
                        Check(button.Left >= 0 && button.Right <= actions.ClientSize.Width && button.Bottom <= actions.ClientSize.Height);
                    foreach (var size in new[] { new Size(1080, 740), new Size(950, 650) })
                    {
                        editor.ClientSize = new Size((int)(size.Width * editor.DeviceDpi / 96f), (int)(size.Height * editor.DeviceDpi / 96f));
                        editor.PerformLayout();
                        Check(input.Height >= input.PreferredHeight && input.Top > 0);
                    }
                    input.Focus(); Check(input.Focused);
                    var heading = controls.OfType<Label>().First(c => c.Font.Size >= 26);
                    typeof(Control).GetMethod("OnMouseDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                        .Invoke(heading, [new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 0)]);
                    Check(!input.Focused);
                    editor.Dispose();
                });
                editor.ShowDialog();
                Console.WriteLine("PASS sidebar action scope, padded search, narrow layout and background focus release");
            }
            var settings = new AppSettings();
            using (var dialog = new SettingsDialog(settings))
            {
                dialog.Shown += (_, _) => dialog.BeginInvoke(() =>
                {
                    foreach (var checkbox in All(dialog).OfType<CheckBox>()) checkbox.Checked = !checkbox.Checked;
                    All(dialog).OfType<Button>().Single(c => c.Text == "保存").PerformClick();
                });
                Check(dialog.ShowDialog() == DialogResult.OK && settings.StartWithWindows && !settings.CloseToTray && !settings.ShowInShortcutGuide);
                Console.WriteLine("PASS real settings form applies toggles on Save");
            }
            if (args.Length == 2 && args[0] == "--startup")
            {
                var service = new StartupService(args[1]); var original = service.GetCommand();
                try
                {
                    service.Set(true); Check(service.GetCommand() == $"\"{args[1]}\" --background");
                    service.Set(false); Check(!service.IsEnabled);
                    Console.WriteLine("PASS current-user startup ON/OFF and quoted EXE path");
                }
                finally { service.Restore(original); }
                Check(service.GetCommand() == original);
            }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static IEnumerable<Control> All(Control control)
    {
        foreach (Control child in control.Controls) { yield return child; foreach (var nested in All(child)) yield return nested; }
    }
    private static void Check(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
}
