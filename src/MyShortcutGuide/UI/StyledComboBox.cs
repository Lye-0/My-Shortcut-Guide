namespace MyShortcutGuide.UI;

internal sealed class StyledComboBox : Control
{
    public List<object> Items { get; } = [];
    private int selected = -1;
    private ToolStripDropDown? popup;
    public event EventHandler? SelectedIndexChanged;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => selected;
        set
        {
            if (value < -1 || value >= Items.Count) throw new ArgumentOutOfRangeException(nameof(value));
            if (selected == value) return; selected = value; Invalidate();
            AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1); SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public object? SelectedItem { get => selected < 0 ? null : Items[selected]; set => SelectedIndex = value is null ? -1 : Items.IndexOf(value); }
    public StyledComboBox()
    {
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = true; Height = 38; Margin = new Padding(0, 0, 0, 8); BackColor = Theme.Surface; ForeColor = Theme.Text;
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Canvas);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var shape = RoundedPanel.Outline(Width, Height, 6 * DeviceDpi / 96f);
        using var fill = new SolidBrush(BackColor); e.Graphics.FillPath(fill, shape);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var scale = DeviceDpi / 96f; int S(int v) => (int)(v * scale);
        TextRenderer.DrawText(e.Graphics, SelectedItem?.ToString() ?? "", Font, new Rectangle(S(12), 0, Math.Max(0, Width - S(48)), Height), Enabled ? Theme.Text : Theme.Muted,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        using var line = new Pen(Focused || popup?.Visible == true ? Theme.Accent : Theme.Border);
        using var shape = RoundedPanel.Outline(Width, Height, 6 * scale); e.Graphics.DrawPath(line, shape);
        using var arrow = new Pen(Theme.Muted, Math.Max(1, scale)); var x = Width - S(19); var y = Height / 2;
        e.Graphics.DrawLines(arrow, [new(x - S(4), y - S(2)), new(x, y + S(2)), new(x + S(4), y - S(2))]);
    }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { Focus(); OpenPopup(); } }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down or Keys.Home or Keys.End || base.IsInputKey(keyData);
    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (keyData is Keys.Enter or Keys.Space or Keys.F4 || keyData == (Keys.Alt | Keys.Down)) { OpenPopup(); return true; }
        return base.ProcessDialogKey(keyData);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Space or Keys.Enter or Keys.F4 || (e.Alt && e.KeyCode == Keys.Down)) { OpenPopup(); e.Handled = e.SuppressKeyPress = true; }
        else if (Items.Count > 0 && e.KeyCode is Keys.Up or Keys.Down or Keys.Home or Keys.End)
        {
            SelectedIndex = e.KeyCode == Keys.Home ? 0 : e.KeyCode == Keys.End ? Items.Count - 1 : Math.Clamp(selected + (e.KeyCode == Keys.Up ? -1 : 1), 0, Items.Count - 1);
            e.Handled = e.SuppressKeyPress = true;
        }
    }
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e); if (char.IsControl(e.KeyChar)) return;
        var index = Items.FindIndex(Math.Min(selected + 1, Items.Count), item => item.ToString()!.StartsWith(e.KeyChar.ToString(), StringComparison.CurrentCultureIgnoreCase));
        if (index < 0) index = Items.FindIndex(item => item.ToString()!.StartsWith(e.KeyChar.ToString(), StringComparison.CurrentCultureIgnoreCase));
        if (index >= 0) SelectedIndex = index; e.Handled = true;
    }
    internal void OpenPopup()
    {
        if (Items.Count == 0 || popup?.Visible == true) return;
        popup?.Dispose();
        var list = new OptionList(selected) { Font = Font, AccessibleName = AccessibleName + "の候補", DrawMode = DrawMode.OwnerDrawFixed, BorderStyle = BorderStyle.None, IntegralHeight = false, BackColor = Theme.Surface, ForeColor = Theme.Text, ItemHeight = (int)(36 * DeviceDpi / 96f) };
        list.Items.AddRange(Items.ToArray()); list.SelectedIndex = selected;
        var height = Math.Min((int)(280 * DeviceDpi / 96f), Items.Count * list.ItemHeight);
        var frame = new ScrollFrame(list) { Size = new Size(Math.Max(Width, 140), height), Margin = Padding.Empty };
        var host = new ToolStripControlHost(frame) { AutoSize = false, Size = frame.Size, Margin = Padding.Empty, Padding = Padding.Empty };
        popup = new ToolStripDropDown { AutoClose = true, AutoSize = true, BackColor = Theme.Surface, Padding = new Padding(2), Margin = Padding.Empty, DropShadowEnabled = false };
        popup.Items.Add(host);
        void Commit() { if (list.SelectedIndex >= 0) SelectedIndex = list.SelectedIndex; popup.Close(); Focus(); }
        list.MouseUp += (_, e) => { if (e.Button == MouseButtons.Left && list.IndexFromPoint(e.Location) >= 0) Commit(); };
        list.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.Handled = e.SuppressKeyPress = true; Commit(); }
            else if (e.KeyCode == Keys.Escape) { e.Handled = e.SuppressKeyPress = true; popup.Close(); Focus(); }
            else if (e.KeyCode == Keys.Tab) { e.Handled = e.SuppressKeyPress = true; Commit(); Parent?.SelectNextControl(this, !e.Shift, true, true, true); }
        };
        popup.Closed += (_, _) => { Invalidate(); AccessibilityNotifyClients(AccessibleEvents.StateChange, -1); };
        popup.Show(this, new Point(0, Height + 4)); list.Focus(); Invalidate();
    }
    protected override AccessibleObject CreateAccessibilityInstance() => new SelectorAccessibility(this);
    private sealed class SelectorAccessibility(StyledComboBox owner) : ControlAccessibleObject(owner)
    {
        public override AccessibleRole Role => AccessibleRole.ComboBox;
        public override string? Value { get => owner.SelectedItem?.ToString() ?? ""; set { } }
        public override string? DefaultAction => "候補を開く";
        public override AccessibleStates State => base.State | (owner.popup?.Visible == true ? AccessibleStates.Expanded : AccessibleStates.Collapsed);
        public override void DoDefaultAction() => owner.OpenPopup();
    }
    protected override void Dispose(bool disposing) { if (disposing) popup?.Dispose(); base.Dispose(disposing); }
    private sealed class OptionList(int committed) : ThemedListBox
    {
        private int hover = -1;
        protected override bool IsInputKey(Keys keyData) => (keyData & Keys.KeyCode) is Keys.Enter or Keys.Escape or Keys.Tab || base.IsInputKey(keyData);
        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); var index = IndexFromPoint(e.Location); if (index != hover) { hover = index; Invalidate(); } }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = -1; Invalidate(); }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return; var active = (e.State & DrawItemState.Selected) != 0;
            using var fill = new SolidBrush(active ? Theme.Selected : hover == e.Index ? Color.FromArgb(44, 49, 57) : Theme.Surface); e.Graphics.FillRectangle(fill, e.Bounds);
            var inset = (int)(12 * DeviceDpi / 96f);
            TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), Font, new Rectangle(inset, e.Bounds.Y, Math.Max(0, e.Bounds.Width - inset * 4), e.Bounds.Height), Theme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (e.Index == committed) TextRenderer.DrawText(e.Graphics, "✓", Font, new Rectangle(e.Bounds.Right - inset * 3, e.Bounds.Y, inset * 2, e.Bounds.Height), Theme.Accent, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            if (active) { using var line = new SolidBrush(Theme.Accent); e.Graphics.FillRectangle(line, 2, e.Bounds.Y + 6, 2, e.Bounds.Height - 12); }
        }
    }
}
