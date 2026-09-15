using System.Drawing.Drawing2D;
using MyShortcutGuide.Core;

namespace MyShortcutGuide.UI;

internal static class Theme
{
    public static readonly Color Canvas = Color.FromArgb(24, 26, 30);
    public static readonly Color Sidebar = Color.FromArgb(30, 32, 37);
    public static readonly Color Surface = Color.FromArgb(37, 40, 46);
    public static readonly Color Border = Color.FromArgb(65, 70, 78);
    public static readonly Color Text = Color.FromArgb(239, 241, 245);
    public static readonly Color Muted = Color.FromArgb(165, 172, 185);
    public static readonly Color Accent = Color.FromArgb(161, 191, 255);
    public static readonly Color Selected = Color.FromArgb(48, 62, 84);
    private static readonly Lazy<Icon> IconCache = new(() => Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? SystemIcons.Application);
    public static Icon AppIcon => IconCache.Value;
    public static Font Font(float size = 10, FontStyle style = FontStyle.Regular) => new("Yu Gothic UI", size, style);
    public static Label Label(string text, float size = 10, Color? color = null) => new()
    {
        Text = text, AutoSize = true, Font = Font(size), ForeColor = color ?? Text,
        Margin = new Padding(0, 0, 0, 8), UseMnemonic = false
    };
    public static Button Button(string text, Action action, bool primary = false)
    {
        var button = new Button
        {
            Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(72, 38),
            Padding = new Padding(12, 4, 12, 4), FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : Surface, ForeColor = primary ? Canvas : Text,
            Margin = new Padding(0, 0, 8, 0), Cursor = Cursors.Hand, AccessibleName = text
        };
        button.FlatAppearance.BorderColor = primary ? Accent : Border;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(186, 207, 255) : Color.FromArgb(51, 56, 65);
        button.Click += (_, _) => action();
        return button;
    }
    public static FlowLayoutPanel Row(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
        panel.Controls.AddRange(controls);
        return panel;
    }
    public static void StyleForm(Form form)
    {
        form.SuspendLayout();
        form.BackColor = Canvas; form.ForeColor = Text; form.Font = Font(); form.Icon = AppIcon;
        form.AutoScaleMode = AutoScaleMode.None;
        form.StartPosition = FormStartPosition.CenterScreen;
    }
    public static void CompleteLayout(Form form)
    {
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.AutoScaleDimensions = new SizeF(96, 96);
        form.ResumeLayout(false);
        form.PerformAutoScale();
        form.PerformLayout();
    }
    public static TextBox Input(string label, string value = "", bool multiline = false) => new()
    {
        Text = multiline ? value.ReplaceLineEndings(Environment.NewLine) : value, AccessibleName = label, Dock = DockStyle.Fill, BackColor = Surface, ForeColor = Text,
        BorderStyle = BorderStyle.FixedSingle, Multiline = multiline,
        Height = multiline ? 84 : 32, Margin = new Padding(0, 0, 0, 16)
    };
    public static void Error(IWin32Window? owner, Exception ex) =>
        MessageBox.Show(owner, ex.Message, "操作を完了できませんでした", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

internal sealed class SectionList : ListBox
{
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ItemHeight = (int)(47 * DeviceDpi / 96f); }
    public SectionList()
    {
        Dock = DockStyle.Fill; DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 47;
        IntegralHeight = false; BorderStyle = BorderStyle.None; BackColor = Theme.Sidebar; ForeColor = Theme.Text;
        AccessibleName = "セクション一覧";
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var section = (Section)Items[e.Index];
        var selected = (e.State & DrawItemState.Selected) != 0;
        using var background = new SolidBrush(selected ? Theme.Selected : Theme.Sidebar);
        e.Graphics.FillRectangle(background, e.Bounds);
        if (selected) { using var accent = new SolidBrush(Theme.Accent); e.Graphics.FillRectangle(accent, e.Bounds.X, e.Bounds.Y + 10, 3, e.Bounds.Height - 20); }
        var text = new Rectangle(e.Bounds.X + 18, e.Bounds.Y, e.Bounds.Width - 65, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, section.Name, Font, text, Theme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(e.Graphics, section.Shortcuts.Count.ToString(), Font, new Rectangle(e.Bounds.Right - 43, e.Bounds.Y, 28, e.Bounds.Height), Theme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        e.DrawFocusRectangle();
    }
}

internal sealed class ShortcutList : ListBox
{
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ItemHeight = (int)(110 * DeviceDpi / 96f); }
    public ShortcutList()
    {
        Dock = DockStyle.Fill; DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 110;
        IntegralHeight = false; BorderStyle = BorderStyle.None; BackColor = Theme.Canvas; ForeColor = Theme.Text;
        AccessibleName = "ショートカット一覧";
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var item = (ShortcutEntry)Items[e.Index];
        var scale = DeviceDpi / 96f;
        int S(int n) => (int)(n * scale);
        var selected = (e.State & DrawItemState.Selected) != 0;
        using var background = new SolidBrush(Theme.Canvas);
        e.Graphics.FillRectangle(background, e.Bounds);
        var card = new Rectangle(e.Bounds.X, e.Bounds.Y + S(4), e.Bounds.Width - S(2), e.Bounds.Height - S(8));
        using var cardBrush = new SolidBrush(selected ? Theme.Selected : Theme.Surface);
        e.Graphics.FillRectangle(cardBrush, card);
        if (selected) { using var accent = new Pen(Theme.Accent); e.Graphics.DrawRectangle(accent, card); }
        using var titleFont = Theme.Font(11, FontStyle.Bold);
        var name = item.Name + (item.Recommended ? "  ★" : "");
        TextRenderer.DrawText(e.Graphics, name, titleFont, new Rectangle(card.X + S(16), card.Y + S(11), card.Width - S(32), S(24)), Theme.Text, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(e.Graphics, item.Description.Replace('\n', ' ').Replace('\r', ' '), Font, new Rectangle(card.X + S(16), card.Y + S(37), card.Width - S(32), S(22)), Theme.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        var x = card.X + S(16);
        foreach (var key in item.Parts)
        {
            var width = TextRenderer.MeasureText(e.Graphics, key, Font).Width + S(12);
            if (x + width > card.Right - S(12)) break;
            var cap = new Rectangle(x, card.Bottom - S(33), width, S(24));
            using var fill = new SolidBrush(Theme.Canvas); e.Graphics.FillRectangle(fill, cap);
            using var border = new Pen(Theme.Border); e.Graphics.DrawRectangle(border, cap);
            TextRenderer.DrawText(e.Graphics, key, Font, cap, Theme.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            x += width + S(6);
        }
        if ((e.State & DrawItemState.Focus) != 0) ControlPaint.DrawFocusRectangle(e.Graphics, card, Theme.Text, Theme.Surface);
    }
}
