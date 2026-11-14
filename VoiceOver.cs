using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Windows.Forms;

internal static class VoiceOver
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT { public uint type; public INPUTUNION data; }
    [StructLayout(LayoutKind.Explicit)]
    internal struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT keyboard;
        [FieldOffset(0)] public MOUSEINPUT mouse;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort key, scan;
        public uint flags, time;
        public UIntPtr extra;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        public int x, y;
        public uint mouseData, flags, time;
        public UIntPtr extra;
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, INPUT[] inputs, int size);
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);
    private delegate bool EnumWindowProc(IntPtr window, IntPtr parameter);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowProc callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr window, int index);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr window, StringBuilder name, int size);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr window);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr window, int attribute, out int value, int size);

    private static bool IsShellClass(string name)
    {
        return name == "Shell_TrayWnd" || name == "Shell_SecondaryTrayWnd" ||
            name == "Progman" || name == "WorkerW" || name == "Windows.UI.Core.CoreWindow";
    }

    private static bool IsTextTarget(IntPtr window)
    {
        if (window == IntPtr.Zero || !IsWindowVisible(window) || IsIconic(window)) return false;
        StringBuilder name = new StringBuilder(256);
        GetClassName(window, name, name.Capacity);
        if (IsShellClass(name.ToString())) return false;
        // Ignore floating toolbars, no-activate surfaces and invisible virtual desktops.
        if ((GetWindowLong(window, -20) & (0x80 | 0x08000000)) != 0) return false;
        int cloaked;
        if (DwmGetWindowAttribute(window, 14, out cloaked, 4) == 0 && cloaked != 0) return false;
        return GetWindowTextLength(window) > 0;
    }

    private static IntPtr FindTextTarget()
    {
        IntPtr foreground = GetForegroundWindow();
        if (IsTextTarget(foreground)) return foreground;
        IntPtr target = IntPtr.Zero;
        // EnumWindows enumerates desktop windows in Z order. After a taskbar
        // click the previously active application is the first ordinary window.
        EnumWindows(delegate(IntPtr window, IntPtr unused)
        {
            if (!IsTextTarget(window)) return true;
            target = window;
            return false;
        }, IntPtr.Zero);
        return target;
    }

    private static bool RestoreTextTarget(IntPtr target)
    {
        if (target == IntPtr.Zero) return false;
        if (GetForegroundWindow() != target) SetForegroundWindow(target);
        // Activation lets the application restore its own focused editor/control.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (GetForegroundWindow() == target) { Thread.Sleep(100); return GetForegroundWindow() == target; }
            Thread.Sleep(25);
        }
        return false;
    }

    private static INPUT Key(ushort key, bool up)
    {
        INPUT input = new INPUT();
        input.type = 1;
        input.data.keyboard.key = key;
        input.data.keyboard.flags = up ? 2u : 0u;
        return input;
    }

    [STAThread]
    private static int Main(string[] args)
    {
        int expected = IntPtr.Size == 8 ? 40 : 28;
        if (args.Length == 1 && args[0] == "--self-test")
            return Marshal.SizeOf(typeof(INPUT)) == expected &&
                IsShellClass("Shell_TrayWnd") && IsShellClass("Shell_SecondaryTrayWnd") &&
                !IsShellClass("Notepad") && !IsShellClass("Chrome_WidgetWin_1") ? 0 : 1;

        bool created;
        using (Mutex mutex = new Mutex(true, "Local\\VoiceOver.Shortcut", out created))
        {
            if (!created) return 0;
            // Wait until the taskbar click and any modifier keys are released.
            // The taskbar itself can take foreground focus even without our own window.
            int waited = 0;
            while (Pressed(1) || Pressed(16) || Pressed(17) || Pressed(18) || Pressed(91) || Pressed(92))
            {
                if (waited >= 3000) return 2;
                Thread.Sleep(25);
                waited += 25;
            }
            Thread.Sleep(120);
            IntPtr target = FindTextTarget();
            if (!RestoreTextTarget(target))
            {
                MessageBox.Show("Не удалось вернуть фокус в текстовое окно. Выберите нужное окно и повторите нажатие кнопки.", "Голосовой ввод", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 4;
            }
            INPUT[] keys = { Key(0x5B, false), Key(0x48, false), Key(0x48, true), Key(0x5B, true) };
            uint sent = SendInput((uint)keys.Length, keys, Marshal.SizeOf(typeof(INPUT)));
            if (sent != keys.Length)
            {
                // Release both injected keys even after a partial insertion.
                INPUT[] release = { Key(0x48, true), Key(0x5B, true) };
                SendInput(2, release, Marshal.SizeOf(typeof(INPUT)));
                MessageBox.Show("Windows не приняла команду голосового ввода. Если текстовое приложение запущено от администратора, откройте его обычным способом.", "Голосовой ввод", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 3;
            }
            Thread.Sleep(300);
            return 0;
        }
    }

    private static bool Pressed(int key) { return (GetAsyncKeyState(key) & 0x8000) != 0; }
}
