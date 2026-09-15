namespace MyShortcutGuide.UI;

internal sealed class ReorderEventArgs(int from, int insertion) : EventArgs
{
    public int From { get; } = from;
    public int Insertion { get; } = insertion;
}

internal class ReorderListBox : ThemedListBox
{
    private Point start;
    private object? item;
    private bool dragging;
    private int insertion = -1;
    private long lastScroll;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ReorderEnabled { get; set; } = true;
    public event EventHandler<ReorderEventArgs>? ReorderRequested;
    public event Action<int>? ItemActivated;
    public event Action? BlankClicked;
    protected virtual Rectangle ItemHitBounds(int index) => GetItemRectangle(index);
    internal int HitTestItem(Point point)
    {
        if (!ClientRectangle.Contains(point)) return -1;
        var index = IndexFromPoint(point);
        return index >= 0 && ItemHitBounds(index).Contains(point) ? index : -1;
    }
    protected override void WndProc(ref Message m)
    {
        // Do not let the native list box start its own mouse tracking loop on
        // the second press: it conflicts with our drag capture and retains selection in blank space.
        if (m.Msg == 0x203)
        {
            var point = new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16)));
            var index = HitTestItem(point); ResetDrag();
            if (index >= 0) { SelectedIndex = index; ItemActivated?.Invoke(index); }
            else BlankClicked?.Invoke();
            return;
        }
        if (m.Msg == 0x201)
        {
            var point = new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16)));
            var index = HitTestItem(point);
            if (index < 0) { ResetDrag(); BlankClicked?.Invoke(); return; }
            Focus(); SelectedIndex = index;
            start = point; item = ReorderEnabled ? Items[index] : null; Capture = item is not null;
            return;
        }
        base.WndProc(ref m);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!ReorderEnabled || item is null || e.Button != MouseButtons.Left) return;
        var threshold = new Rectangle(start.X - SystemInformation.DragSize.Width / 2, start.Y - SystemInformation.DragSize.Height / 2, SystemInformation.DragSize.Width, SystemInformation.DragSize.Height);
        if (!dragging && threshold.Contains(e.Location)) return;
        dragging = true; Cursor = Cursors.SizeNS;
        var edge = (int)(28 * DeviceDpi / 96f);
        if (ClientRectangle.Contains(e.Location) && Environment.TickCount64 - lastScroll > 90 && (e.Y < edge || e.Y > Height - edge))
        {
            Offset += e.Y < edge ? -1 : 1; lastScroll = Environment.TickCount64;
        }
        var index = IndexFromPoint(e.Location);
        var next = !ClientRectangle.Contains(e.Location) ? -1 : index < 0 ? Items.Count : index + (e.Y >= GetItemRectangle(index).Top + ItemHeight / 2 ? 1 : 0);
        if (next != insertion) { insertion = next; Invalidate(); }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        var from = item is null ? -1 : Items.IndexOf(item); var target = insertion;
        var commit = dragging && e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location) && ReorderEnabled;
        ResetDrag(); base.OnMouseUp(e);
        if (commit && from >= 0 && target >= 0) ReorderRequested?.Invoke(this, new ReorderEventArgs(from, target));
    }
    private void ResetDrag() { item = null; dragging = false; insertion = -1; Capture = false; Cursor = Cursors.Default; Invalidate(); }
    protected override void OnMouseCaptureChanged(EventArgs e) { base.OnMouseCaptureChanged(e); if (!Capture && item is not null) ResetDrag(); }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape && item is not null) { ResetDrag(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    protected void DrawInsertion(DrawItemEventArgs e)
    {
        if (insertion < 0 || (e.Index != insertion && !(insertion == Items.Count && e.Index == Items.Count - 1))) return;
        var y = e.Index == insertion ? e.Bounds.Top + 1 : e.Bounds.Bottom - 2;
        using var pen = new Pen(Theme.Accent, Math.Max(2, DeviceDpi / 48f));
        e.Graphics.DrawLine(pen, e.Bounds.Left + 5, y, e.Bounds.Right - 5, y);
    }
}
