using MyShortcutGuide.Core;

namespace MyShortcutGuide.UI;

internal class DialogBase : Form
{
    protected readonly BackgroundSurface Content = new() { Dock = DockStyle.Fill, BackColor = Theme.Canvas, ColumnCount = 1, AutoScroll = true, Padding = new Padding(28, 22, 28, 16) };
    protected readonly FlowLayoutPanel Footer = new() { Dock = DockStyle.Bottom, Height = 70, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(20, 12, 20, 16) };
    protected readonly Label ErrorLabel = Theme.Label("", 10, Color.FromArgb(255, 180, 160));
    protected DialogBase(string title, Size size)
    {
        Theme.StyleForm(this); Text = title; ClientSize = size; MinimumSize = new Size(size.Width, size.Height);
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false; MaximizeBox = false; MinimizeBox = false;
        Controls.Add(new ScrollFrame(Content) { Dock = DockStyle.Fill }); Controls.Add(Footer);
    }
    protected void AddField(string label, Control control, int height)
    {
        var fieldLabel = Theme.Label(label); fieldLabel.Tag = control;
        var row = Content.RowCount++; Content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); Content.Controls.Add(fieldLabel, 0, row);
        row = Content.RowCount++; Content.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        Content.Controls.Add(control is ThemedTextBox { Multiline: true } ? new ScrollFrame(control) { Dock = DockStyle.Fill } : control, 0, row);
    }
    protected override void OnLoad(EventArgs e)
    {
        Theme.CompleteLayout(this);
        base.OnLoad(e);
    }
    protected void AddContent(Control control, int height)
    {
        var row = Content.RowCount++; Content.RowStyles.Add(new RowStyle(SizeType.Absolute, height)); Content.Controls.Add(control, 0, row);
    }
    protected void AddButtons(Action save)
    {
        var button = Theme.Button("保存", save, true); var cancel = Theme.Button("キャンセル", () => DialogResult = DialogResult.Cancel);
        cancel.DialogResult = DialogResult.Cancel; Footer.Controls.Add(button); Footer.Controls.Add(cancel);
        AcceptButton = button; CancelButton = cancel;
        ErrorLabel.AutoSize = false; ErrorLabel.Dock = DockStyle.Fill; AddContent(ErrorLabel, 55);
    }
}

internal sealed class NameDialog : DialogBase
{
    private readonly TextBox input;
    private NameDialog(string title, string label, string value) : base(title, new Size(460, 250))
    {
        input = Theme.Input(label, value); input.MaxLength = 80; AddField(label + "（必須）", input, 46);
        AddButtons(() =>
        {
            if (string.IsNullOrWhiteSpace(input.Text)) { ErrorLabel.Text = "セクション名を入力してください。"; input.Focus(); return; }
            DialogResult = DialogResult.OK;
        });
        Shown += (_, _) => { input.Focus(); input.SelectAll(); };
    }
    public static string? Ask(IWin32Window owner, string title, string label, string value)
    {
        using var dialog = new NameDialog(title, label, value);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.input.Text.Trim() : null;
    }
}

internal sealed class SettingsDialog : DialogBase
{
    public SettingsDialog(AppSettings settings) : base("設定", new Size(590, 485))
    {
        AddContent(Theme.Label("いつでも、すぐ使えるように", 19), 54);
        var start = new CheckBox { Text = "Windowsログイン時に自動起動", AutoSize = true, Checked = settings.StartWithWindows };
        var close = new CheckBox { Text = "閉じるボタンで通知領域へ", AutoSize = true, Checked = settings.CloseToTray };
        var show = new CheckBox { Text = "Shortcut Guideに My Shortcuts を表示", AutoSize = true, Checked = settings.ShowInShortcutGuide };
        AddContent(start, 44); AddContent(close, 36);
        AddContent(Theme.Label("オフの場合はタスクバーへ最小化します。終了は Exit から。", 9, Theme.Muted), 38);
        AddContent(show, 44);
        AddContent(Theme.Label("変更は「保存」で反映されます。", 10, Theme.Muted), 40);
        AddButtons(() => { settings.StartWithWindows = start.Checked; settings.CloseToTray = close.Checked; settings.ShowInShortcutGuide = show.Checked; DialogResult = DialogResult.OK; });
    }
}

internal sealed class ShortcutDialog : DialogBase
{
    private readonly TextBox name, description;
    private readonly StyledComboBox section, key;
    private readonly CheckBox win, ctrl, shift, alt, recommended;
    private readonly Label preview = Theme.Label("", 18, Theme.Accent);
    public ShortcutEntry Entry { get; }
    public Guid SectionId => ((Section)section.SelectedItem!).Id;
    public ShortcutDialog(List<Section> sections, Guid sectionId, ShortcutEntry entry, bool existing)
        : base(existing ? "ショートカットを編集" : "ショートカットを追加", new Size(640, 790))
    {
        Entry = entry;
        AddContent(Theme.Label(existing ? "ショートカットを編集" : "新しいショートカット", 22), 58);
        name = Theme.Input("名前", entry.Name); name.MaxLength = 120; AddField("名前（必須）", name, 45);
        description = Theme.Input("説明", entry.Description, true); description.MaxLength = 2000; description.ScrollBars = ScrollBars.Vertical; AddField("説明（任意）", description, 85);
        section = Combo("セクション"); section.Items.AddRange(sections.Cast<object>().ToArray()); section.SelectedItem = sections.Single(s => s.Id == sectionId);
        AddField("セクション", section, 46);
        win = Modifier("Win", entry.Win); ctrl = Modifier("Ctrl", entry.Ctrl); shift = Modifier("Shift", entry.Shift); alt = Modifier("Alt", entry.Alt);
        AddField("修飾キー", Theme.Row(win, ctrl, shift, alt), 42);
        key = Combo("メインキー"); key.Items.AddRange(KeyCatalog.All.Cast<object>().ToArray()); key.SelectedItem = KeyCatalog.All.First(k => k.Code == entry.KeyCode);
        key.Width = 260; key.Dock = DockStyle.None; key.Margin = new Padding(0, 0, 12, 8);
        key.SelectedIndexChanged += (_, _) => UpdatePreview();
        var record = Theme.Button("キーを記録", RecordKeys);
        AddField("メインキー", Theme.Row(key, record), 49);
        preview.AutoSize = false; preview.Dock = DockStyle.Fill; AddContent(preview, 52);
        recommended = new CheckBox { Text = "おすすめに表示（Recommended）", AutoSize = true, Checked = entry.Recommended }; AddContent(recommended, 40);
        AddButtons(() =>
        {
            Entry.Name = name.Text.Trim(); Entry.Description = description.Text.Trim();
            Entry.Win = win.Checked; Entry.Ctrl = ctrl.Checked; Entry.Shift = shift.Checked; Entry.Alt = alt.Checked;
            Entry.KeyCode = ((KeyOption)key.SelectedItem!).Code; Entry.Recommended = recommended.Checked;
            try
            {
                new ShortcutDocument { Sections = [new Section { Name = "Validate", Shortcuts = [Entry] }] }.Validate();
                DialogResult = DialogResult.OK;
            }
            catch (InvalidDataException ex) { ErrorLabel.Text = ex.Message; name.Focus(); }
        });
        UpdatePreview(); Shown += (_, _) => name.Focus();
    }
    private static StyledComboBox Combo(string name) => new()
    {
        AccessibleName = name, Dock = DockStyle.Fill
    };
    private CheckBox Modifier(string text, bool value)
    {
        var box = new CheckBox { Text = text, Checked = value, AutoSize = true, Margin = new Padding(0, 0, 22, 0) };
        box.CheckedChanged += (_, _) => UpdatePreview(); return box;
    }
    private void UpdatePreview()
    {
        if (key?.SelectedItem is not KeyOption option) return;
        preview.Text = new ShortcutEntry { KeyCode = option.Code, Win = win.Checked, Ctrl = ctrl.Checked, Shift = shift.Checked, Alt = alt.Checked }.Gesture;
    }
    private void RecordKeys()
    {
        using var dialog = new CaptureDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is not { } result) return;
        win.Checked = result.Win; ctrl.Checked = result.Ctrl; shift.Checked = result.Shift; alt.Checked = result.Alt;
        key.SelectedItem = KeyCatalog.All.First(k => k.Code == result.KeyCode); UpdatePreview();
    }
}
