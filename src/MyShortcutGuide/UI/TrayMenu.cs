using System.Drawing.Drawing2D;

namespace MyShortcutGuide.UI;

internal sealed class TrayMenu : ContextMenuStrip
{
    private readonly Font menuFont = Theme.Font();
    public TrayMenu(Action openEditor, Action openGuide, Action exit)
    {
        Font = menuFont; BackColor = Theme.Sidebar; ForeColor = Theme.Text;
        ShowImageMargin = false; ShowCheckMargin = false; DropShadowEnabled = false;
        Padding = new Padding(6); Renderer = new MenuRenderer();
        Items.Add(new ToolStripLabel("MY SHORTCUT GUIDE") { ForeColor = Theme.Muted });
        AddAction("Open Editor", "エディターを開く", "\uE70F", openEditor);
        AddAction("Open Shortcut Guide", "Shortcut Guideを開く", "\uE765", openGuide);
        Items.Add(new ToolStripSeparator());
        AddAction("Exit", "常駐アプリを終了", "\uE8BB", exit);
        Opening += (_, _) =>
        {
            var scale = DeviceDpi / 96f;
            Padding = new Padding((int)(6 * scale));
            foreach (ToolStripItem item in Items)
            {
                item.AutoSize = false;
                item.Size = new Size((int)(264 * scale), (int)((item is ToolStripSeparator ? 13 : item is ToolStripLabel ? 34 : 44) * scale));
            }
        };
    }
    private void AddAction(string text, string accessibleName, string glyph, Action action)
    {
        var item = new ToolStripMenuItem(text) { AccessibleName = accessibleName, Tag = glyph };
        item.Click += (_, _) => action(); Items.Add(item);
    }
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        using var shape = RoundedPanel.Outline(Width, Height, 8 * DeviceDpi / 96f);
        var old = Region; Region = new Region(shape); old?.Dispose();
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); if (disposing) menuFont.Dispose();
    }
    private sealed class MenuRenderer : ToolStripRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) => e.Graphics.Clear(Theme.Sidebar);
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shape = RoundedPanel.Outline(e.ToolStrip.Width, e.ToolStrip.Height, 8 * (e.ToolStrip?.DeviceDpi ?? 96) / 96f);
            using var border = new Pen(Theme.Border); e.Graphics.DrawPath(border, shape);
        }
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected || !e.Item.Enabled) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shape = RoundedPanel.Outline(e.Item.Width, e.Item.Height, 5 * (e.ToolStrip?.DeviceDpi ?? 96) / 96f);
            using var fill = new SolidBrush(Theme.Selected); e.Graphics.FillPath(fill, shape);
        }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            var scale = (e.ToolStrip?.DeviceDpi ?? 96) / 96f; int S(int v) => (int)(v * scale);
            var header = e.Item is ToolStripLabel;
            if (e.Item.Tag is string glyph)
            {
                using var iconFont = new Font("Segoe Fluent Icons", 11);
                TextRenderer.DrawText(e.Graphics, glyph, iconFont, new Rectangle(S(12), 0, S(24), e.Item.Height), e.Item.Selected ? Theme.Accent : Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            using var headerFont = Theme.Font(8);
            TextRenderer.DrawText(e.Graphics, e.Text, header ? headerFont : e.TextFont,
                new Rectangle(S(header ? 12 : 46), 0, e.Item.Width - S(header ? 24 : 58), e.Item.Height),
                header || !e.Item.Enabled ? Theme.Muted : Theme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var inset = (int)(12 * (e.ToolStrip?.DeviceDpi ?? 96) / 96f);
            using var line = new Pen(Theme.Border); e.Graphics.DrawLine(line, inset, e.Item.Height / 2, e.Item.Width - inset, e.Item.Height / 2);
        }
    }
}
