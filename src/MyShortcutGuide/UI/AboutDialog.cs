using MyShortcutGuide.Core;
using MyShortcutGuide.Services;

namespace MyShortcutGuide.UI;

internal sealed class AboutDialog : DialogBase
{
    private readonly ToolTip tips = new();
    public AboutDialog(StartupService startup, Action<string>? copyText = null) : base("アプリ情報", new Size(720, 760))
    {
        AddAuto(Theme.Label("My Shortcut Guide", 22));
        var version = typeof(AboutDialog).Assembly.GetName().Version;
        AddContent(Theme.Label($"Version {version?.ToString(3)}  ·  GPL-3.0-only", 10, Theme.Muted), 36);
        AddContent(Theme.Label("この環境で使用している場所", 12), 38);
        var command = startup.GetCommand();
        copyText ??= Clipboard.SetText;
        var rows = new (string Title, string Value, string? Copy)[]
        {
            ("アプリ本体", Environment.ProcessPath ?? AppContext.BaseDirectory, Environment.ProcessPath ?? AppContext.BaseDirectory),
            ("ショートカット・アプリ設定（JSON）", AppPaths.Json, AppPaths.Json),
            ("直前のデータのバックアップ", AppPaths.Json + ".bak" + (File.Exists(AppPaths.Json + ".bak") ? "" : "  （未作成）"), AppPaths.Json + ".bak"),
            ("Shortcut Guide用の生成ファイル（YAML）", Path.Combine(AppPaths.Manifests, ManifestWriter.FileName), Path.Combine(AppPaths.Manifests, ManifestWriter.FileName)),
            ("Windowsログイン時の自動起動", command is null ? "オフ（登録なし）" : "オン", null),
            ("自動起動の保存先（レジストリの値）", StartupService.RegistryLocation, StartupService.RegistryLocation),
            ("登録されている起動コマンド", command ?? "登録なし", command),
        };
        foreach (var (title, value, copyValue) in rows)
        {
            AddAuto(Theme.Label(title, 10, Theme.Muted));
            var text = Theme.Label(value, 10); text.MaximumSize = new Size(565, 0);
            text.AccessibleName = title; text.TabStop = false; text.Margin = new Padding(0, 5, 12, 5);
            var row = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top,
                ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 0, 0, 16) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize)); row.Controls.Add(text, 0, 0);
            if (copyValue is not null)
            {
                var copy = Theme.IconButton("\uE8C8", title + "をコピー", () =>
                {
                    try { copyText(copyValue); ErrorLabel.ForeColor = Theme.Accent; ErrorLabel.Text = title + "をコピーしました"; }
                    catch (Exception ex) { ErrorLabel.ForeColor = Color.FromArgb(255, 180, 160); ErrorLabel.Text = "コピーできませんでした。もう一度お試しください。"; System.Diagnostics.Debug.WriteLine(ex); }
                });
                copy.Tag = copyValue; copy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                tips.SetToolTip(copy, title + "をコピー"); row.Controls.Add(copy, 1, 0);
            }
            AddAuto(row);
        }
        var note = Theme.Label("場所は実行環境から取得しています。自動起動は現在のWindowsユーザーに対する設定です。", 9, Theme.Muted);
        note.MaximumSize = new Size(625, 0); AddAuto(note);
        var moveInstructions = Theme.Label("1. 通知領域の右クリックメニューから Exit を選び、アプリを終了します。\n\n2. MyShortcutGuide.exe が入ったフォルダーを、移動先へ移します。EXEの名前は変更しないでください。\n\n3. 移動先の MyShortcutGuide.exe を起動します。登録済みデータは現在のWindowsユーザーの保存先から読み込まれます。\n\n4. 自動起動を利用する場合は、設定で「Windowsログイン時に自動起動」をオンにして保存します。すでにオンでも保存すると、移動先のEXEへ登録が更新されます。\n\nタスクバーやデスクトップの起動用ショートカットが古い場所を指している場合は、作り直してください。この手順は同じPC・同じWindowsユーザー内での本体移動です。別の環境へデータを渡す場合はExport / Importを使います。", 10);
        moveInstructions.MaximumSize = new Size(625, 0);
        moveInstructions.Margin = new Padding(12, 10, 12, 16); moveInstructions.Visible = false;
        Button? moveToggle = null;
        moveToggle = Theme.Button("▸  本体の移動方法", () =>
        {
            moveInstructions.Visible = !moveInstructions.Visible;
            moveToggle!.Text = moveInstructions.Visible ? "▾  本体の移動方法" : "▸  本体の移動方法";
            moveToggle.AccessibleDescription = moveInstructions.Visible ? "展開済み" : "折りたたみ";
            Content.PerformLayout(); UpdateScrollExtent();
        });
        moveToggle.AccessibleName = "本体の移動方法"; moveToggle.AccessibleDescription = "折りたたみ";
        moveToggle.Dock = DockStyle.Fill; moveToggle.Margin = new Padding(0, 10, 0, 0);
        AddAuto(moveToggle); AddAuto(moveInstructions);
        var close = Theme.Button("閉じる", () => DialogResult = DialogResult.OK);
        Footer.Controls.Add(close); AcceptButton = close; CancelButton = close;
        Disposed += (_, _) => tips.Dispose();
        Shown += (_, _) => UpdateScrollExtent();
        ErrorLabel.AutoSize = false; ErrorLabel.Dock = DockStyle.Fill; AddContent(ErrorLabel, 38);
    }
    private void UpdateScrollExtent()
    {
        Content.AutoScrollMinSize = new Size(0, Content.GetRowHeights().Sum() + Content.Padding.Vertical);
    }
    private void AddAuto(Control control)
    {
        var row = Content.RowCount++; Content.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Content.Controls.Add(control, 0, row);
    }
}
