namespace MyShortcutGuide.UI;

internal sealed class NoticeDialog : DialogBase
{
    public NoticeDialog(string title, string message, string action = "閉じる", bool confirmation = false, bool danger = false)
        : base(title, new Size(590, 350))
    {
        AddContent(Theme.Label(danger ? "削除の確認" : confirmation ? "内容の確認" : "お知らせ", 10, danger ? Color.FromArgb(255, 177, 175) : Theme.Accent), 34);
        AddContent(Theme.Label(title, 21), 62);
        var body = Theme.Label(message.ReplaceLineEndings(Environment.NewLine), 11);
        body.MaximumSize = new Size(500, 0); body.AccessibleName = body.Text;
        var row = Content.RowCount++; Content.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Content.Controls.Add(body, 0, row);
        var accept = Theme.Button(action, () => DialogResult = DialogResult.OK, !danger);
        if (danger)
        {
            accept.BackColor = Color.FromArgb(105, 47, 54); accept.ForeColor = Color.FromArgb(255, 225, 225);
            accept.FlatAppearance.BorderColor = Color.FromArgb(169, 81, 90);
            accept.FlatAppearance.MouseOverBackColor = Color.FromArgb(133, 54, 65);
        }
        Footer.Controls.Add(accept);
        if (confirmation)
        {
            var cancel = Theme.Button("キャンセル", () => DialogResult = DialogResult.Cancel);
            Footer.Controls.Add(cancel); CancelButton = cancel; AcceptButton = cancel;
            Shown += (_, _) => cancel.Focus();
        }
        else { AcceptButton = accept; CancelButton = accept; Shown += (_, _) => accept.Focus(); }
    }
    public static bool Confirm(IWin32Window? owner, string title, string message, string action, bool danger = false)
    {
        using var dialog = new NoticeDialog(title, message, action, true, danger);
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }
    public static void ShowMessage(IWin32Window? owner, string title, string message)
    {
        using var dialog = new NoticeDialog(title, message);
        dialog.ShowDialog(owner);
    }
}
