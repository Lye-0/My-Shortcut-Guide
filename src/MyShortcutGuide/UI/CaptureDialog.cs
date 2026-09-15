using System.Runtime.InteropServices;
using MyShortcutGuide.Core;

namespace MyShortcutGuide.UI;

/// <summary>A hook exists only while this modal has focus. No hook is installed by the resident process.</summary>
internal sealed class CaptureDialog : DialogBase
{
    private readonly Label display = Theme.Label("キーを押してください", 21, Theme.Accent);
    private readonly HookProc hookProc;
    private readonly HashSet<int> held = [];
    private nint hook;
    public ShortcutEntry? Result { get; private set; }
    public CaptureDialog() : base("キーを記録", new Size(575, 340))
    {
        hookProc = OnKey;
        AddContent(Theme.Label("登録するキーの組み合わせを押す", 17), 50);
        AddContent(display, 67);
        AddContent(Theme.Label("すべてのキーを離すと確定します。Escも登録できます。\nCtrl + Alt + Delete など、Windowsが保護する操作は手動で選択してください。", 10, Theme.Muted), 80);
        Footer.Controls.Add(Theme.Button("キャンセル", () => DialogResult = DialogResult.Cancel));
        Shown += (_, _) =>
        {
            hook = SetWindowsHookEx(13, hookProc, GetModuleHandle(null), 0);
            if (hook == 0) { Theme.Error(this, new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error())); DialogResult = DialogResult.Cancel; }
        };
        Deactivate += (_, _) => { Release(); if (Visible && DialogResult == DialogResult.None) DialogResult = DialogResult.Cancel; };
    }
    private nint OnKey(int code, nint wParam, nint lParam)
    {
        if (code < 0 || GetForegroundWindow() != Handle) return CallNextHookEx(hook, code, wParam, lParam);
        var vk = Marshal.ReadInt32(lParam);
        var down = wParam == 0x100 || wParam == 0x104;
        var up = wParam == 0x101 || wParam == 0x105;
        if (down)
        {
            held.Add(vk);
            if (Result is null && KeyCatalog.IsValid(vk))
            {
                Result = new ShortcutEntry { KeyCode = vk, Win = IsDown(0x5B, 0x5C), Ctrl = IsDown(0x11, 0xA2, 0xA3), Shift = IsDown(0x10, 0xA0, 0xA1), Alt = IsDown(0x12, 0xA4, 0xA5) };
                BeginInvoke(() => display.Text = Result.Gesture);
            }
        }
        else if (up)
        {
            held.Remove(vk);
            if (Result is not null && held.Count == 0) BeginInvoke(() => { Release(); DialogResult = DialogResult.OK; });
        }
        return down || up ? 1 : CallNextHookEx(hook, code, wParam, lParam);
    }
    private bool IsDown(params int[] keys) => keys.Any(k => held.Contains(k) || (GetAsyncKeyState(k) & 0x8000) != 0);
    private void Release() { if (hook != 0) { UnhookWindowsHookEx(hook); hook = 0; } }
    protected override void Dispose(bool disposing) { Release(); base.Dispose(disposing); }
    private delegate nint HookProc(int code, nint wParam, nint lParam);
    [DllImport("user32.dll", SetLastError = true)] private static extern nint SetWindowsHookEx(int id, HookProc callback, nint module, uint threadId);
    [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook, int code, nint wParam, nint lParam);
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern nint GetModuleHandle(string? name);
}
