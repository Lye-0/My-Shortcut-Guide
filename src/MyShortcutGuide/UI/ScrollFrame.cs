using System.Runtime.InteropServices;

namespace MyShortcutGuide.UI;

internal interface IScrollContent
{
    int Total { get; }
    int Page { get; }
    int Offset { get; set; }
    int Step { get; }
    event Action? ViewChanged;
}
internal static class ScrollNative
{
    [DllImport("user32.dll")] internal static extern bool ShowScrollBar(nint window, int bar, bool show);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern nint GetWindowLongPtr(nint window, int index);
    [DllImport("user32.dll")] internal static extern nint SendMessage(nint window, int message, nint wParam, nint lParam);
    internal static int Wheel(nint value) => (short)((long)value >> 16);
    internal static int Units(ref int remainder, int delta, int page, int step)
    {
        remainder += delta; var ticks = remainder / 120; remainder %= 120;
        return ticks * (SystemInformation.MouseWheelScrollLines < 0 ? page : SystemInformation.MouseWheelScrollLines * step);
    }
    internal static void HideNative(nint window)
    {
        if (((long)GetWindowLongPtr(window, -16) & 0x200000) != 0) ShowScrollBar(window, 1, false);
    }
}
internal sealed class ScrollFrame : Panel
{
    public ScrollFrame(Control content)
    {
        if (content is not IScrollContent source) throw new ArgumentException("Scrollable content is required.");
        BackColor = content.BackColor; Margin = content.Margin; content.Margin = Padding.Empty;
        var rail = new ScrollRail(source) { Dock = DockStyle.Right, Width = 14, BackColor = content.BackColor };
        content.Dock = DockStyle.Fill; Controls.Add(content); Controls.Add(rail);
        source.ViewChanged += rail.Invalidate;
        Disposed += (_, _) => source.ViewChanged -= rail.Invalidate;
    }
}
internal sealed class ScrollRail(IScrollContent source) : Control
{
    private bool dragging, hover;
    private int grab, wheelRemainder;
    protected override void OnCreateControl()
    {
        base.OnCreateControl(); SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        TabStop = false; AccessibleRole = AccessibleRole.ScrollBar; AccessibleName = "縦スクロール";
    }
    private int Max => Math.Max(0, source.Total - source.Page);
    protected override AccessibleObject CreateAccessibilityInstance() => new RailAccessibility(this);
    private sealed class RailAccessibility(ScrollRail owner) : ControlAccessibleObject(owner)
    {
        public override string? Value { get => owner.CurrentOffset.ToString(); set { if (int.TryParse(value, out var offset)) owner.SetOffset(offset); } }
        public override string? DefaultAction => "次のページへ";
        public override void DoDefaultAction() => owner.NextPage();
    }
    private int CurrentOffset => source.Offset;
    private void SetOffset(int value) { source.Offset = Math.Clamp(value, 0, Max); Invalidate(); }
    private void NextPage() => SetOffset(source.Offset + source.Page);
    private Rectangle Thumb
    {
        get
        {
            var track = Math.Max(1, Height - 8);
            var length = Math.Min(track, Math.Max((int)(30 * DeviceDpi / 96f), track * source.Page / Math.Max(1, source.Total)));
            var top = 4 + (Max == 0 ? 0 : (track - length) * Math.Clamp(source.Offset, 0, Max) / Max);
            return new Rectangle(4, top, Math.Max(3, Width - 8), length);
        }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); if (Max == 0) return;
        using var brush = new SolidBrush(dragging ? Theme.Accent : hover ? Theme.Muted : Color.FromArgb(87, 94, 107));
        e.Graphics.FillRectangle(brush, Thumb);
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hover = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = false; Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e); if (e.Button != MouseButtons.Left || Max == 0) return;
        if (source is Control content) content.Focus();
        if (Thumb.Contains(e.Location)) { dragging = true; grab = e.Y - Thumb.Top; Capture = true; }
        else source.Offset = Math.Clamp(source.Offset + (e.Y < Thumb.Top ? -source.Page : source.Page), 0, Max);
        Invalidate();
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); if (!dragging) return;
        source.Offset = Math.Clamp((e.Y - grab - 4) * Max / Math.Max(1, Height - 8 - Thumb.Height), 0, Max); Invalidate();
    }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); dragging = false; Capture = false; Invalidate(); }
    protected override void OnMouseCaptureChanged(EventArgs e) { base.OnMouseCaptureChanged(e); if (!Capture) dragging = false; }
    protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); source.Offset = Math.Clamp(source.Offset - ScrollNative.Units(ref wheelRemainder, e.Delta, source.Page, source.Step), 0, Max); Invalidate(); }
}
internal class ThemedListBox : ListBox, IScrollContent
{
    private int wheelRemainder;
    protected override CreateParams CreateParams { get { var p = base.CreateParams; p.Style &= ~0x200000; return p; } }
    private bool updating;
    public event Action? ViewChanged;
    public int Total => Items.Count;
    public int Page => Math.Max(1, ClientSize.Height / Math.Max(1, ItemHeight));
    public int Step => 1;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Offset { get => Items.Count == 0 ? 0 : TopIndex; set { if (Items.Count > 0) TopIndex = Math.Clamp(value, 0, Math.Max(0, Total - Page)); ViewChanged?.Invoke(); } }
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x20A) { Offset -= ScrollNative.Units(ref wheelRemainder, ScrollNative.Wheel(m.WParam), Page, Step); return; }
        base.WndProc(ref m);
        if (!updating && IsHandleCreated && m.Msg is 0x5 or 0x115 or 0x100 or 0x180 or 0x184 or 0x186 or 0x197)
        {
            updating = true;
            try { ScrollNative.HideNative(Handle); ViewChanged?.Invoke(); }
            finally { updating = false; }
        }
    }
}
internal sealed class ThemedTextBox : TextBox, IScrollContent
{
    private int wheelRemainder;
    protected override CreateParams CreateParams { get { var p = base.CreateParams; p.Style &= ~0x200000; return p; } }
    private bool updating;
    public event Action? ViewChanged;
    public int Total => !IsHandleCreated ? 1 : GetLineFromCharIndex(TextLength) + 1;
    public int Page => Math.Max(1, ClientSize.Height / Math.Max(1, Font.Height));
    public int Step => 1;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Offset
    {
        get => !IsHandleCreated ? 0 : (int)ScrollNative.SendMessage(Handle, 0xCE, 0, 0);
        set { if (IsHandleCreated) ScrollNative.SendMessage(Handle, 0xB6, 0, Math.Clamp(value, 0, Math.Max(0, Total - Page)) - Offset); ViewChanged?.Invoke(); }
    }
    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); ViewChanged?.Invoke(); }
    protected override void WndProc(ref Message m)
    {
        if (Multiline && m.Msg == 0x20A) { Offset -= ScrollNative.Units(ref wheelRemainder, ScrollNative.Wheel(m.WParam), Page, Step); return; }
        base.WndProc(ref m);
        if (!updating && Multiline && IsHandleCreated && m.Msg is 0x5 or 0x115 or 0x100 or 0x102 or 0xB6 or 0xB7)
        {
            updating = true;
            try { ScrollNative.HideNative(Handle); ViewChanged?.Invoke(); }
            finally { updating = false; }
        }
    }
}
