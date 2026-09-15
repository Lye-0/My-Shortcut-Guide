using MyShortcutGuide.Services;
using MyShortcutGuide.UI;

namespace MyShortcutGuide;

internal sealed class TrayContext : ApplicationContext
{
    private readonly AppController controller;
    private readonly Form dispatcher = new() { ShowInTaskbar = false };
    private readonly NotifyIcon tray;
    private readonly RegisteredWaitHandle activation;
    private EditorForm? editor;
    private bool disposed;
    public TrayContext(EventWaitHandle signal, bool background)
    {
        controller = new AppController();
        _ = dispatcher.Handle;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Editor", null, (_, _) => ShowEditor());
        menu.Items.Add("Open Shortcut Guide", null, (_, _) => OpenGuide());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => Exit());
        tray = new NotifyIcon { Text = "My Shortcut Guide", Icon = Theme.AppIcon, Visible = true, ContextMenuStrip = menu };
        tray.DoubleClick += (_, _) => ShowEditor();
        activation = ThreadPool.RegisterWaitForSingleObject(signal, (_, _) =>
        {
            try { if (!disposed) dispatcher.BeginInvoke(ShowEditor); }
            catch (InvalidOperationException) { /* Shutdown raced with activation. */ }
        }, null, Timeout.Infinite, false);
        dispatcher.BeginInvoke(async () =>
        {
            if (!background) ShowEditor();
            try { await controller.InitializeAsync(); }
            catch (Exception ex) { Theme.Error(editor, ex); }
        });
    }
    public void ShowEditor()
    {
        if (disposed) return;
        // An active modal editor owns its focus; a second launch must not bypass it.
        if (editor is { IsDisposed: false, Enabled: false })
        {
            editor.OwnedForms.LastOrDefault()?.Activate();
            return;
        }
        if (editor is null || editor.IsDisposed)
        {
            editor = new EditorForm(controller, Exit, OpenGuide);
            editor.FormClosed += (_, _) => editor = null;
        }
        editor.Show();
        editor.WindowState = FormWindowState.Normal;
        editor.Activate();
        editor.BringToFront();
    }
    private void OpenGuide()
    {
        try { controller.PowerToys.Open(); }
        catch (Exception ex) { Theme.Error(editor, ex); }
    }
    private void Exit()
    {
        if (controller.IsBusy)
        {
            NoticeDialog.ShowMessage(editor, "保存処理を実行中です", "保存処理が完了してから終了してください。");
            return;
        }
        ExitThread();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;
            activation.Unregister(null);
            tray.Visible = false;
            tray.ContextMenuStrip?.Dispose();
            tray.Dispose();
            editor?.Dispose();
            dispatcher.Dispose();
        }
        base.Dispose(disposing);
    }
}
