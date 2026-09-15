using MyShortcutGuide.Core;
using MyShortcutGuide.UI;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware); Application.EnableVisualStyles(); Application.SetDefaultFont(Theme.Font()); Application.SetColorMode(SystemColorMode.Dark);
        using var form = new Form { Text = "My Shortcut Guide · 表示検証", ClientSize = new Size(1080, 900) };
        Theme.StyleForm(form);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), FlowDirection = FlowDirection.TopDown };
        panel.Controls.Add(Theme.Button("候補一覧・スクロールの検証", () =>
        {
            var sections = Enumerable.Range(1, 40).Select(i => new Section { Name = $"セクション {i:00}" }).ToList();
            using var dialog = new ShortcutDialog(sections, sections[0].Id,
                new ShortcutEntry { Name = "表示検証用サンプル", Description = string.Join(Environment.NewLine, Enumerable.Range(1, 60).Select(i => $"説明文のスクロール確認 {i:00}")), Ctrl = true, KeyCode = 70 }, false);

            dialog.ShowInTaskbar = true; dialog.ShowDialog(form);
        }));
        panel.Controls.Add(Theme.Button("削除確認の検証", () =>
        {
            using var dialog = new NoticeDialog("セクションを削除しますか？", "「Creative」\n登録済みのショートカット：0 件", "削除する", true, true);
            dialog.ShowInTaskbar = true; dialog.ShowDialog(form);
        }));
        panel.Controls.Add(Theme.Button("ドラッグ並べ替えの検証", () =>
        {
            using var dragForm = new Form { Text = "ドラッグ検証", ClientSize = new Size(900, 600), ShowInTaskbar = true };
            Theme.StyleForm(dragForm);
            var sections = new SectionList(); var shortcuts = new ShortcutList();
            var sectionData = Enumerable.Range(1, 12).Select(i => new Section { Name = $"Section {i:00}" }).ToList();
            var shortcutData = Enumerable.Range(1, 12).Select(i => new ShortcutEntry { Name = $"Shortcut {i:00}", Ctrl = true, KeyCode = 65 + i }).ToList();
            sections.Items.AddRange(sectionData.Cast<object>().ToArray()); shortcuts.Items.AddRange(shortcutData.Cast<object>().ToArray());
            void SaveOrder() => File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "drag-result.txt"), string.Join(",", sectionData.Select(s => s.Name)) + "\n" + string.Join(",", shortcutData.Select(s => s.Name)));
            sections.ReorderRequested += (_, e) => { ListOrder.Move(sectionData, e.From, e.Insertion); sections.Items.Clear(); sections.Items.AddRange(sectionData.Cast<object>().ToArray()); SaveOrder(); };
            shortcuts.ReorderRequested += (_, e) => { ListOrder.Move(shortcutData, e.From, e.Insertion); shortcuts.Items.Clear(); shortcuts.Items.AddRange(shortcutData.Cast<object>().ToArray()); SaveOrder(); };
            dragForm.Controls.Add(new ScrollFrame(shortcuts) { Dock = DockStyle.Fill });
            dragForm.Controls.Add(new ScrollFrame(sections) { Dock = DockStyle.Left, Width = 270 });
            Theme.CompleteLayout(dragForm); dragForm.ShowDialog(form);
        }));
        form.Controls.Add(panel); Theme.CompleteLayout(form);
        form.Shown += (_, _) => form.BeginInvoke(() => (!args.Contains("--form") ? panel.Controls.OfType<Button>().Last() : panel.Controls.OfType<Button>().First()).PerformClick());
        Application.Run(form);
    }
    private static IEnumerable<Control> All(Control parent)
    {
        foreach (Control child in parent.Controls) { yield return child; foreach (var nested in All(child)) yield return nested; }
    }
}
