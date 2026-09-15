using MyShortcutGuide.Core;
using MyShortcutGuide.Services;

namespace MyShortcutGuide.UI;

internal sealed class EditorForm : Form
{
    private readonly AppController controller;
    private readonly Action exit;
    private readonly SectionList sections = new();
    private readonly ShortcutList shortcuts = new();
    private readonly Label heading = Theme.Label("Favorites", 26);
    private readonly Label subtitle = Theme.Label("", color: Theme.Muted);
    private readonly Label status = Theme.Label("", 9, Theme.Muted);
    private readonly Label empty = Theme.Label("まだショートカットがありません\n「＋ ショートカット」で、最初のキーを登録しましょう。", 12, Theme.Muted);
    private readonly TextBox search = Theme.Input("選択セクション内を検索");
    private readonly List<Control> mutations = [];
    private Button edit = null!, remove = null!, up = null!, down = null!, add = null!;
    private Button sectionRename = null!, sectionRemove = null!, sectionUp = null!, sectionDown = null!;
    private bool refreshing;
    private Section? SelectedSection => sections.SelectedItem as Section;
    private ShortcutEntry? SelectedShortcut => shortcuts.SelectedItem as ShortcutEntry;

    public EditorForm(AppController controller, Action exit, Action openGuide)
    {
        this.controller = controller; this.exit = exit;
        Text = "My Shortcut Guide"; Theme.StyleForm(this); ClientSize = new Size(1080, 740); MinimumSize = new Size(950, 650);
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270)); shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(shell);
        var sidebar = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar, Padding = new Padding(18, 28, 18, 18), ColumnCount = 1, RowCount = 6 };
        sidebar.RowStyles.Add(new(SizeType.Absolute, 102)); sidebar.RowStyles.Add(new(SizeType.Absolute, 32));
        sidebar.RowStyles.Add(new(SizeType.Percent, 100)); sidebar.RowStyles.Add(new(SizeType.Absolute, 112));
        sidebar.RowStyles.Add(new(SizeType.Absolute, 1)); sidebar.RowStyles.Add(new(SizeType.Absolute, 108));
        shell.Controls.Add(sidebar, 0, 0);
        var brand = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        brand.Controls.Add(Theme.Label("MY SHORTCUT GUIDE", 10, Theme.Accent));
        brand.Controls.Add(Theme.Label("いつもの操作を、\nすぐ手元に。", 17));
        sidebar.Controls.Add(brand, 0, 0); sidebar.Controls.Add(Theme.Label("ライブラリ", 9, Theme.Muted), 0, 1);
        sidebar.Controls.Add(sections, 0, 2);
        var sectionActions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 12, 0, 0) };
        var sectionAdd = Theme.Button("＋ セクション", () => Run(() => AddSectionAsync()));
        sectionRename = Theme.Button("名前変更", () => Run(RenameSectionAsync));
        sectionRemove = Theme.Button("削除", () => Run(DeleteSectionAsync));
        sectionUp = Theme.Button("↑", () => Run(() => MoveSectionAsync(-1))); sectionUp.MinimumSize = new Size(38, 38); sectionUp.AccessibleName = "セクションを上へ";
        sectionDown = Theme.Button("↓", () => Run(() => MoveSectionAsync(1))); sectionDown.MinimumSize = new Size(38, 38); sectionDown.AccessibleName = "セクションを下へ";
        sectionActions.Controls.Add(Theme.Row(sectionAdd, sectionUp, sectionDown));
        var row2 = Theme.Row(sectionRename, sectionRemove); row2.Padding = new Padding(0, 8, 0, 0); sectionActions.Controls.Add(row2);
        sidebar.Controls.Add(sectionActions, 0, 3);
        sidebar.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Theme.Border }, 0, 4);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 18, 0, 0) };
        var settings = Theme.Button("設定", () => Run(SettingsAsync));
        bottom.Controls.Add(Theme.Row(settings, Theme.Button("Exit", exit)));
        var resident = Theme.Label("●  バックグラウンドで待機", 9, Theme.Muted); resident.Margin = new Padding(0, 12, 0, 0); bottom.Controls.Add(resident);
        sidebar.Controls.Add(bottom, 0, 5);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(30, 26, 30, 18), ColumnCount = 1, RowCount = 7 };
        foreach (var h in new[] { 48, 100, 58, 44 }) main.RowStyles.Add(new(SizeType.Absolute, h));
        main.RowStyles.Add(new(SizeType.Percent, 100)); main.RowStyles.Add(new(SizeType.Absolute, 62)); main.RowStyles.Add(new(SizeType.Absolute, 48));
        shell.Controls.Add(main, 1, 0);
        var import = Theme.Button("Import", () => Run(ImportAsync)); var export = Theme.Button("Export", Export);
        var guide = Theme.Button("Shortcut Guide を開く", openGuide);
        main.Controls.Add(Theme.Row(guide, import, export), 0, 0);
        var titles = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 14, 0, 0) };
        heading.Margin = Padding.Empty; titles.Controls.Add(heading); titles.Controls.Add(subtitle); main.Controls.Add(titles, 0, 1);
        add = Theme.Button("＋ ショートカット", () => Run(() => EditShortcutAsync(false)), true);
        var regenerate = Theme.Button("ガイドへ再反映", () => Run(controller.RegenerateAsync));
        main.Controls.Add(Theme.Row(add, regenerate), 0, 2);
        search.PlaceholderText = "このセクションを検索"; search.Margin = new Padding(0, 0, 0, 12);
        search.TextChanged += (_, _) => RefreshShortcuts(); main.Controls.Add(search, 0, 3);
        var listPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        listPanel.Controls.Add(shortcuts); empty.Dock = DockStyle.Fill; empty.TextAlign = ContentAlignment.MiddleCenter; empty.AutoSize = false;
        listPanel.Controls.Add(empty); main.Controls.Add(listPanel, 0, 4);
        edit = Theme.Button("編集", () => Run(() => EditShortcutAsync(true)));
        remove = Theme.Button("削除", () => Run(DeleteShortcutAsync));
        up = Theme.Button("↑ 上へ", () => Run(() => MoveShortcutAsync(-1)));
        down = Theme.Button("↓ 下へ", () => Run(() => MoveShortcutAsync(1)));
        var itemActions = Theme.Row(edit, remove, up, down); itemActions.Padding = new Padding(0, 14, 0, 0); main.Controls.Add(itemActions, 0, 5);
        status.AutoSize = false; status.Dock = DockStyle.Fill; main.Controls.Add(status, 0, 6);
        mutations.AddRange([sectionAdd, sectionRename, sectionRemove, sectionUp, sectionDown, settings, import, regenerate, add, edit, remove, up, down]);
        sections.SelectedIndexChanged += (_, _) => { if (!refreshing) { search.Clear(); RefreshShortcuts(); } };
        shortcuts.SelectedIndexChanged += (_, _) => UpdateActions();
        shortcuts.DoubleClick += (_, _) => { if (SelectedShortcut is not null) Run(() => EditShortcutAsync(true)); };
        controller.Changed += OnChanged;
        FormClosed += (_, _) => controller.Changed -= OnChanged;
        FormClosing += (_, e) =>
        {
            if (e.CloseReason != CloseReason.UserClosing) return;
            if (controller.IsBusy) { e.Cancel = true; return; }
            if (!controller.Document.Settings.CloseToTray) { e.Cancel = true; WindowState = FormWindowState.Minimized; }
        };
        RefreshAll();
        Theme.CompleteLayout(this);
    }
    private void OnChanged(object? sender, EventArgs e) { if (!IsDisposed) RefreshAll(); }
    private void RefreshAll(Guid? sectionId = null, Guid? shortcutId = null)
    {
        sectionId ??= SelectedSection?.Id; shortcutId ??= SelectedShortcut?.Id;
        refreshing = true; sections.BeginUpdate(); sections.Items.Clear(); sections.Items.AddRange(controller.Document.Sections.Cast<object>().ToArray());
        sections.SelectedIndex = Math.Max(-1, controller.Document.Sections.FindIndex(s => s.Id == sectionId));
        if (sections.SelectedIndex < 0 && sections.Items.Count > 0) sections.SelectedIndex = 0;
        sections.EndUpdate(); refreshing = false; RefreshShortcuts(shortcutId);
        status.Text = controller.Status; status.ForeColor = controller.HasWarning ? Color.FromArgb(244, 194, 121) : Theme.Muted;
        UpdateActions();
    }
    private void RefreshShortcuts() => RefreshShortcuts(null);
    private void RefreshShortcuts(Guid? id)
    {
        id ??= SelectedShortcut?.Id;
        var section = SelectedSection;
        heading.Text = section?.Name ?? "ライブラリ";
        subtitle.Text = $"{section?.Shortcuts.Count ?? 0} 件のショートカット";
        var query = search.Text.Trim();
        shortcuts.BeginUpdate(); shortcuts.Items.Clear();
        if (section is not null) shortcuts.Items.AddRange(section.Shortcuts.Where(s => string.IsNullOrEmpty(query) || $"{s.Name} {s.Description} {s.Gesture}".Contains(query, StringComparison.OrdinalIgnoreCase)).Cast<object>().ToArray());
        for (var i = 0; i < shortcuts.Items.Count; i++) if (((ShortcutEntry)shortcuts.Items[i]).Id == id) shortcuts.SelectedIndex = i;
        shortcuts.EndUpdate();
        empty.Text = query.Length > 0 ? "一致するショートカットがありません" : section is null ? "「＋ セクション」でライブラリを作りましょう。" : "まだショートカットがありません\n「＋ ショートカット」で、最初のキーを登録しましょう。";
        empty.Visible = shortcuts.Items.Count == 0; shortcuts.Visible = !empty.Visible; UpdateActions();
    }
    private void UpdateActions()
    {
        foreach (var control in mutations) control.Enabled = !controller.IsBusy;
        var s = SelectedSection; var i = SelectedShortcut; var ready = !controller.IsBusy;
        add.Enabled = ready && s is not null;
        edit.Enabled = remove.Enabled = ready && i is not null;
        up.Enabled = ready && i is not null && search.Text.Length == 0 && s!.Shortcuts.IndexOf(i) > 0;
        down.Enabled = ready && i is not null && search.Text.Length == 0 && s!.Shortcuts.IndexOf(i) < s.Shortcuts.Count - 1;
        sectionRename.Enabled = sectionRemove.Enabled = ready && s is not null;
        sectionUp.Enabled = ready && sections.SelectedIndex > 0;
        sectionDown.Enabled = ready && s is not null && sections.SelectedIndex < sections.Items.Count - 1;
    }
    private async void Run(Func<Task> action)
    {
        if (controller.IsBusy) return;
        try { await action(); }
        catch (Exception ex) { Theme.Error(this, ex); }
    }
    private async Task AddSectionAsync()
    {
        var name = NameDialog.Ask(this, "セクションを作成", "セクション名", ""); if (name is null) return;
        var doc = DocumentStore.Clone(controller.Document); var section = new Section { Name = name }; doc.Sections.Add(section);
        await controller.SaveAsync(doc); RefreshAll(section.Id);
    }
    private async Task RenameSectionAsync()
    {
        if (SelectedSection is not { } s) return;
        var name = NameDialog.Ask(this, "セクション名を変更", "セクション名", s.Name); if (name is null) return;
        var doc = DocumentStore.Clone(controller.Document); doc.Sections.Single(x => x.Id == s.Id).Name = name; await controller.SaveAsync(doc);
    }
    private async Task DeleteSectionAsync()
    {
        if (SelectedSection is not { } s || MessageBox.Show(this, $"「{s.Name}」と、その中の{s.Shortcuts.Count}件を削除しますか？", "セクションの削除", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;
        var doc = DocumentStore.Clone(controller.Document); doc.Sections.RemoveAll(x => x.Id == s.Id); await controller.SaveAsync(doc);
    }
    private async Task MoveSectionAsync(int delta)
    {
        var doc = DocumentStore.Clone(controller.Document); var index = sections.SelectedIndex; var target = index + delta;
        if (index < 0 || target < 0 || target >= doc.Sections.Count) return;
        (doc.Sections[index], doc.Sections[target]) = (doc.Sections[target], doc.Sections[index]); await controller.SaveAsync(doc);
    }
    private async Task EditShortcutAsync(bool existing)
    {
        if (SelectedSection is not { } section || (existing && SelectedShortcut is null)) return;
        var doc = DocumentStore.Clone(controller.Document);
        var item = existing ? doc.Sections.SelectMany(s => s.Shortcuts).Single(s => s.Id == SelectedShortcut!.Id) : new ShortcutEntry();
        using var dialog = new ShortcutDialog(doc.Sections, section.Id, item, existing);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        foreach (var s in doc.Sections) s.Shortcuts.RemoveAll(x => x.Id == item.Id);
        var target = doc.Sections.Single(s => s.Id == dialog.SectionId);
        var originalIndex = existing && dialog.SectionId == section.Id ? section.Shortcuts.FindIndex(x => x.Id == item.Id) : target.Shortcuts.Count;
        target.Shortcuts.Insert(Math.Clamp(originalIndex, 0, target.Shortcuts.Count), dialog.Entry);
        await controller.SaveAsync(doc); search.Clear(); RefreshAll(target.Id, item.Id);
    }
    private async Task DeleteShortcutAsync()
    {
        if (SelectedShortcut is not { } item || MessageBox.Show(this, $"「{item.Name}」を削除しますか？", "ショートカットの削除", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;
        var doc = DocumentStore.Clone(controller.Document); foreach (var s in doc.Sections) s.Shortcuts.RemoveAll(x => x.Id == item.Id); await controller.SaveAsync(doc);
    }
    private async Task MoveShortcutAsync(int delta)
    {
        if (SelectedShortcut is not { } item || search.Text.Length > 0) return;
        var doc = DocumentStore.Clone(controller.Document); var list = doc.Sections.Single(s => s.Id == SelectedSection!.Id).Shortcuts;
        var index = list.FindIndex(s => s.Id == item.Id); var target = index + delta; if (target < 0 || target >= list.Count) return;
        (list[index], list[target]) = (list[target], list[index]); await controller.SaveAsync(doc);
    }
    private async Task SettingsAsync()
    {
        var doc = DocumentStore.Clone(controller.Document); doc.Settings.StartWithWindows = controller.Startup.IsEnabled;
        using var dialog = new SettingsDialog(doc.Settings); if (dialog.ShowDialog(this) == DialogResult.OK) await controller.SaveAsync(doc, true);
    }
    private async Task ImportAsync()
    {
        using var picker = new OpenFileDialog { Filter = "My Shortcut Guide JSON|*.json", Title = "JSONをインポート" };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        var doc = DocumentStore.Read(picker.FileName);
        if (MessageBox.Show(this, $"{doc.Sections.Count} セクション / {doc.Sections.Sum(s => s.Shortcuts.Count)} 件で現在のライブラリを置き換えます。\n直前のデータはバックアップされます。アプリ設定は現在の設定を引き継ぎます。", "インポートの確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;
        doc.Settings = DocumentStore.Clone(controller.Document).Settings; await controller.SaveAsync(doc);
    }
    private void Export()
    {
        using var picker = new SaveFileDialog { Filter = "My Shortcut Guide JSON|*.json", FileName = "my-shortcuts.json", DefaultExt = "json" };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            if (string.Equals(Path.GetFullPath(picker.FileName), Path.GetFullPath(AppPaths.Json), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("エクスポート先には、アプリの保存先とは別のファイルを選んでください。");
            DocumentStore.Export(controller.Document, picker.FileName); status.Text = "エクスポートしました";
        }
        catch (Exception ex) { Theme.Error(this, ex); }
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.F)) { search.Focus(); return true; }
        if (keyData == (Keys.Control | Keys.N)) { Run(() => EditShortcutAsync(false)); return true; }
        if (keyData == Keys.F2 && shortcuts.Focused) { Run(() => EditShortcutAsync(true)); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
