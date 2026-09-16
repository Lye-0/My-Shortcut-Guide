using System.Runtime.InteropServices;
namespace MyShortcutGuide.Services;

internal static class KeyboardLabels
{
    [DllImport("user32.dll")] internal static extern nint GetKeyboardLayout(uint threadId);
    [DllImport("user32.dll", EntryPoint = "MapVirtualKeyExW")] private static extern uint MapVirtualKeyEx(uint code, uint type, nint layout);
    internal static string? Resolve(int code) => Resolve(code, GetKeyboardLayout(0));
    internal static string? Resolve(int code, nint layout)
    {
        if (code is not (>= 186 and <= 192 or >= 219 and <= 222 or 226)) return null;
        var character = (char)(MapVirtualKeyEx((uint)code, 2, layout) & 0xffff);
        return character != '\0' && !char.IsControl(character) ? character.ToString() : $"VK {code}";
    }
}
