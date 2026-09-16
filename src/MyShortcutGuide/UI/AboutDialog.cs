using MyShortcutGuide.Core;
using MyShortcutGuide.Services;

namespace MyShortcutGuide.UI;

internal sealed class AboutDialog : DialogBase
{
    internal string Details { get; }
    public AboutDialog(StartupService startup) : base("アプリ情報", new Size(720, 760))
    {
        AddAuto(Theme.Label("My Shortcut Guide", 22));
        var version = typeof(AboutDialog).Assembly.GetName().Version;
        AddContent(Theme.Label($"Version {version?.ToString(3)}  ·  MIT License", 10, Theme.Muted), 36);
        AddContent(Theme.Label("この環境で使用している場所", 12), 38);
        var command = startup.GetCommand();
        var rows = new (string Title, string Value)[]
        {
            ("アプリ本体", Environment.ProcessPath ?? AppContext.BaseDirectory),
            ("ショートカット・アプリ設定（JSON）", AppPaths.Json),
            ("直前のデータのバックアップ", AppPaths.Json + ".bak" + (File.Exists(AppPaths.Json + ".bak") ? "" : "  （未作成）")),
            ("Shortcut Guide用の生成ファイル（YAML）", Path.Combine(AppPaths.Manifests, ManifestWriter.FileName)),
            ("Windowsログイン時の自動起動", command is null ? "オフ（登録なし）" : "オン"),
            ("自動起動の保存先（レジストリの値）", StartupService.RegistryLocation),
            ("登録されている起動コマンド", command ?? "登録なし"),
        };
        Details = string.Join(Environment.NewLine + Environment.NewLine, rows.Select(row => row.Title + Environment.NewLine + row.Value));
        foreach (var (title, value) in rows)
        {
            AddAuto(Theme.Label(title, 10, Theme.Muted));
            var text = Theme.Label(value, 10); text.MaximumSize = new Size(625, 0);
            text.Margin = new Padding(0, 0, 0, 20); text.AccessibleName = title; text.TabStop = false;
            AddAuto(text);
        }
        var note = Theme.Label("場所は実行環境から取得しています。自動起動は現在のWindowsユーザーに対する設定です。", 9, Theme.Muted);
        note.MaximumSize = new Size(625, 0); AddAuto(note);
        var close = Theme.Button("閉じる", () => DialogResult = DialogResult.OK);
        var copy = Theme.Button("情報をまとめてコピー", () =>
        {
            try { Clipboard.SetText(Details); ErrorLabel.ForeColor = Theme.Accent; ErrorLabel.Text = "情報をコピーしました"; }
            catch (Exception ex) { ErrorLabel.ForeColor = Color.FromArgb(255, 180, 160); ErrorLabel.Text = "コピーできませんでした。もう一度お試しください。"; System.Diagnostics.Debug.WriteLine(ex); }
        });
        Footer.Controls.Add(close); Footer.Controls.Add(copy); AcceptButton = close; CancelButton = close;
        ErrorLabel.AutoSize = false; ErrorLabel.Dock = DockStyle.Fill; AddContent(ErrorLabel, 38);
    }
    private void AddAuto(Control control)
    {
        var row = Content.RowCount++; Content.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Content.Controls.Add(control, 0, row);
    }
}
