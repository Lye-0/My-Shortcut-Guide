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
                    controls.OfType<StyledComboBox>().Single(c => c.AccessibleName == "メインキー").SelectedItem = KeyCatalog.All.Single(k => k.Code == 70);
                    Check(!controls.OfType<StyledComboBox>().Any(c => c.AccessibleName == "セクション"));
                    Check(controls.OfType<StyledComboBox>().All(c => c.AccessibilityObject.Role == AccessibleRole.ComboBox));
                    controls.OfType<CheckBox>().Single(c => c.Text.Contains("Recommended")).Checked = true;
                    foreach (var label in controls.OfType<Label>().Where(c => c.AutoSize && c.Visible))
                        if (label.Height < label.PreferredHeight) throw new Exception($"Clipped label: {label.Text}: {label.Height}/{label.PreferredHeight}");
                    controls.OfType<Button>().Single(c => c.Text == "保存").PerformClick();
                });
                var result = dialog.ShowDialog();
                Check(result == DialogResult.OK && entry.Ctrl && entry.KeyCode == 70 && entry.Name == "検索: 日本語" && entry.Recommended && dialog.SectionId == sections[0].Id);
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
                    var list = controls.OfType<ShortcutList>().Single();
                    // Use the actual editor wiring without saving or changing user data.
                    var savedItems = list.Items.Cast<object>().ToArray();
                    list.Items.Clear(); list.Items.Add(new ShortcutEntry { Name = "Hit feedback", KeyCode = 65 });
                    list.Visible = true; list.BringToFront();
                    foreach (var reorder in new[] { true, false })
                    {
                        list.ReorderEnabled = reorder;
                        list.SelectedIndex = 0; list.Focus();
                        var y = list.GetItemRectangle(0).Bottom + 20;
                        ScrollNative.SendMessage(list.Handle, 0x201, 1, (nint)((y << 16) | 20));
                        ScrollNative.SendMessage(list.Handle, 0x202, 0, (nint)((y << 16) | 20));
                        Check(list.SelectedIndex == -1 && !list.Focused && !list.Capture);
                        Check(!controls.OfType<Button>().Single(c => c.Text == "編集").Enabled);
                        Check(!controls.OfType<Button>().Single(c => c.Text == "削除").Enabled);
                        ScrollNative.SendMessage(list.Handle, 0x201, 1, (nint)((30 << 16) | 20));
                        ScrollNative.SendMessage(list.Handle, 0x202, 0, (nint)((30 << 16) | 20));
                        Check(list.SelectedIndex == 0 && list.Focused && !list.Capture);
                    }
                    list.Items.Clear(); list.Items.AddRange(savedItems);
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
            foreach (var accept in new[] { false, true })
            {
                using var notice = new NoticeDialog("セクションを削除しますか？", "「テスト用」\n登録済みのショートカット：0 件", "削除する", true, true);
                notice.Shown += (_, _) => notice.BeginInvoke(() =>
                {
                    Check(notice.AcceptButton is Button { Text: "キャンセル" });
                    Check(notice.CancelButton is Button { Text: "キャンセル" });
                    All(notice).OfType<Button>().Single(b => b.Text == (accept ? "削除する" : "キャンセル")).PerformClick();
                });
                Check(notice.ShowDialog() == (accept ? DialogResult.OK : DialogResult.Cancel));
            }
            Console.WriteLine("PASS themed confirmation results and safe default action");
            using (var scrollTest = new Form { ClientSize = new Size(480, 260) })
            {
                var list = new SectionList(); list.Items.AddRange(Enumerable.Range(1, 80).Select(i => new Section { Name = "Section " + i }).Cast<object>().ToArray());
                var text = new ThemedTextBox { Multiline = true, Text = string.Join("\r\n", Enumerable.Range(1, 80)), Height = 100 };
                scrollTest.Controls.Add(new ScrollFrame(list) { Dock = DockStyle.Fill });
                scrollTest.Controls.Add(new ScrollFrame(text) { Dock = DockStyle.Bottom, Height = 100 });
                scrollTest.Shown += (_, _) => scrollTest.BeginInvoke(() =>
                {
                    Check(list.Total > list.Page && text.Total > text.Page);
                    list.Offset = 35; Check(list.Offset == 35);
                    text.Offset = 20; Check(text.Offset == 20);
                    list.Offset = 10000; Check(list.Offset == list.Total - list.Page);
                    list.Offset = 0; text.Offset = 0; Check(list.Offset == 0 && text.Offset == 0);
                    scrollTest.Close();
                });
                scrollTest.ShowDialog();
            }
            Console.WriteLine("PASS themed list/text scroll range and end positions");
            using (var selectorTest = new Form { ClientSize = new Size(480, 400) })
            {
                var selector = new StyledComboBox { Size = new Size(320, 42), Location = new Point(20, 20) };
                selector.Items.AddRange(Enumerable.Range(1, 40).Select(i => (object)("Choice " + i))); selector.SelectedIndex = 0;
                selectorTest.Controls.Add(selector);
                selectorTest.Shown += (_, _) => selectorTest.BeginInvoke(() =>
                {
                    var popupField = typeof(StyledComboBox).GetField("popup", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                    var keyMethod = typeof(Control).GetMethod("OnKeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                    selector.OpenPopup();
                    var popup = (ToolStripDropDown)popupField.GetValue(selector)!;
                    var list = All(((ToolStripControlHost)popup.Items[0]).Control).OfType<ThemedListBox>().Single();
                    Check(popup.Visible && list.Total > list.Page);
                    list.SelectedIndex = 5; keyMethod.Invoke(list, [new KeyEventArgs(Keys.Enter)]);
                    Check(selector.SelectedIndex == 5 && !popup.Visible);
                    selector.OpenPopup(); popup = (ToolStripDropDown)popupField.GetValue(selector)!;
                    list = All(((ToolStripControlHost)popup.Items[0]).Control).OfType<ThemedListBox>().Single();
                    list.Offset = 25; Check(list.Offset == 25); list.SelectedIndex = 30;
                    keyMethod.Invoke(list, [new KeyEventArgs(Keys.Escape)]);
                    Check(selector.SelectedIndex == 5 && !popup.Visible);
                    selectorTest.Close();
                });
                selectorTest.ShowDialog();
            }
            Console.WriteLine("PASS popup commit/cancel and candidate list scrolling");
            using (var hitForm = new Form { ClientSize = new Size(500, 400) })
            {
                var list = new ShortcutList(); list.Items.Add(new ShortcutEntry { Name = "Selected", KeyCode = 65 });
                hitForm.Controls.Add(list); var activations = 0;
                list.ItemActivated += _ => activations++;
                hitForm.Shown += (_, _) => hitForm.BeginInvoke(() =>
                {
                    list.SelectedIndex = 0;
                    void Send(int message, int x, int y) => ScrollNative.SendMessage(list.Handle, message, message == 0x202 ? 0 : 1, (nint)((y << 16) | x));
                    var row = list.GetItemRectangle(0);
                    foreach (var point in new[] { new Point(30, row.Bottom + 30), new Point(30, row.Top + 1), new Point(list.Width - 1, 30) })
                    {
                        Send(0x201, point.X, point.Y); Send(0x202, point.X, point.Y);
                        Send(0x203, point.X, point.Y); Send(0x202, point.X, point.Y);
                        Check(activations == 0 && !list.Capture);
                    }
                    Send(0x201, 30, 30); Send(0x202, 30, 30); Check(activations == 0);
                    Send(0x203, 30, 30); Send(0x202, 30, 30); Check(activations == 1 && !list.Capture);
                    list.ReorderEnabled = false;
                    Send(0x203, 30, row.Bottom + 30); Check(activations == 1);
                    Send(0x203, 30, 30); Check(activations == 2 && !list.Capture);
                    hitForm.Close();
                });
                hitForm.ShowDialog();
            }
            Console.WriteLine("PASS selected shortcut ignores blank space and card gaps; activation only inside card; capture released");
            string? copied = null;
            using (var about = new AboutDialog(new StartupService(), value => copied = value))
            {
                about.Shown += (_, _) => about.BeginInvoke(() =>
                {
                    Check(!All(about).OfType<TextBox>().Any());
                    var copyButtons = All(about).OfType<Button>().Where(b => b.Tag is string).ToArray();
                    Check(copyButtons.Length >= 5);
                    Check(copyButtons.Any(b => (string)b.Tag! == AppPaths.Json));
                    Check(copyButtons.Any(b => (string)b.Tag! == Environment.ProcessPath));
                    Check(copyButtons.Any(b => (string)b.Tag! == StartupService.RegistryLocation));
                    foreach (var button in copyButtons)
                    {
                        button.PerformClick(); Check(copied == (string)button.Tag!);
                        Check(button.Right <= button.Parent!.ClientSize.Width);
                    }
                    copyButtons.Single(b => b.AccessibleName == "直前のデータのバックアップをコピー").PerformClick();
                    Check(copied == AppPaths.Json + ".bak");
                    foreach (var label in All(about).OfType<Label>().Where(c => c.AutoSize && c.Visible))
                        if(label.Height < label.PreferredHeight) throw new Exception($"Clipped: {label.Text}: {label.Height}/{label.PreferredHeight}");
                    Check(!All(about).OfType<Button>().Any(c => c.Text == "情報をまとめてコピー"));
                    var toggle = All(about).OfType<Button>().Single(b => b.AccessibleName == "本体の移動方法");
                    toggle.PerformClick(); Check(toggle.AccessibleDescription == "展開済み");
                    Check(All(about).OfType<Label>().Any(l => l.Visible && l.Text.Contains("移動先へ移します")));
                    toggle.PerformClick(); Check(toggle.AccessibleDescription == "折りたたみ");
                    about.Close();
                });
                about.ShowDialog();
            }
            Console.WriteLine("PASS About displays runtime locations without editable inputs and wraps labels");
            var trayActions = new int[3];
            using (var menuHost = new Form { ClientSize = new Size(500, 400) })
            using (var menu = new TrayMenu(() => trayActions[0]++, () => trayActions[1]++, () => trayActions[2]++))
            {
                menuHost.Shown += (_, _) => menuHost.BeginInvoke(() =>
                {
                    menu.Show(menuHost, new Point(20, 20));
                    Check(menu.Visible && menu.Region is not null);
                    var actions = menu.Items.OfType<ToolStripMenuItem>().ToArray();
                    Check(actions.Length == 3 && actions.All(i => i.Width > 200 && i.Height >= 40));
                    foreach (var action in actions) action.PerformClick();
                    Check(trayActions.SequenceEqual(new[] { 1, 1, 1 }));
                    menu.Close(); Check(!menu.Visible); menuHost.Close();
                });
                menuHost.ShowDialog();
            }
            Console.WriteLine("PASS themed tray menu layout and all three actions");
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
