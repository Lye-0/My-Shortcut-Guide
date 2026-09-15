using MyShortcutGuide.Core;
using MyShortcutGuide.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
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
            dialog.Shown += (_, _) => dialog.BeginInvoke(() => All(dialog).OfType<StyledComboBox>().First().OpenPopup());
            dialog.ShowInTaskbar = true; dialog.ShowDialog(form);
        }));
        panel.Controls.Add(Theme.Button("削除確認の検証", () =>
        {
            using var dialog = new NoticeDialog("セクションを削除しますか？", "「Creative」\n登録済みのショートカット：0 件", "削除する", true, true);
            dialog.ShowInTaskbar = true; dialog.ShowDialog(form);
        }));
        form.Controls.Add(panel); Theme.CompleteLayout(form);
        form.Shown += (_, _) => form.BeginInvoke(() => panel.Controls.OfType<Button>().Last().PerformClick());
        Application.Run(form);
    }
    private static IEnumerable<Control> All(Control parent)
    {
        foreach (Control child in parent.Controls) { yield return child; foreach (var nested in All(child)) yield return nested; }
    }
}
