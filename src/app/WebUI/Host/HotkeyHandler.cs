using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;

namespace Metatool.WebViewHost
{
    public class HotkeyHandler : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly int _hotkeyId;
        private readonly IntPtr _hwnd;
        private readonly HwndSource _source;
        private bool _disposed;

        public event Action? HotkeyPressed;

        public HotkeyHandler(Window window, int hotkeyId, ModifierKeys modifiers, Key key)
        {
            _hotkeyId = hotkeyId;
            _hwnd = new WindowInteropHelper(window).Handle;
            _source = HwndSource.FromHwnd(_hwnd);

            var fsModifiers = ConvertModifiers(modifiers);
            var vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            var registered = RegisterHotKey(_hwnd, _hotkeyId, fsModifiers, vk);
            Debug.WriteLine($"RegisterHotKey(hwnd={_hwnd}, id={_hotkeyId}, mods=0x{fsModifiers:X}, vk={vk}) = {registered}");

            if (!registered)
            {
                var err = Marshal.GetLastWin32Error();
                Debug.WriteLine($"RegisterHotKey failed with error code: {err}");
            }

            _source.AddHook(WndProc);
        }

        private static uint ConvertModifiers(ModifierKeys modifiers)
        {
            uint result = 0;
            if (modifiers.HasFlag(ModifierKeys.Control))
                result |= MOD_CONTROL;
            if (modifiers.HasFlag(ModifierKeys.Shift))
                result |= MOD_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Alt))
                result |= 0x0001; // MOD_ALT
            if (modifiers.HasFlag(ModifierKeys.Windows))
                result |= 0x0008; // MOD_WIN
            return result;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();
                Debug.WriteLine($"WM_HOTKEY received: hotkeyId={hotkeyId}, _hotkeyId={_hotkeyId}");
                if (hotkeyId == _hotkeyId)
                {
                    Debug.WriteLine("Hotkey matched, invoking HotkeyPressed event");
                    HotkeyPressed?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _source.RemoveHook(WndProc);
            UnregisterHotKey(_hwnd, _hotkeyId);
        }
    }
}
