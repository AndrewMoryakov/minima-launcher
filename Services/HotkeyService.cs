using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Сервис для регистрации глобальных горячих клавиш
    /// </summary>
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private IntPtr _windowHandle;
        private HwndSource _source;
        private int _hotkeyId;

        public event EventHandler<HotkeyPressedEventArgs> HotkeyPressed;

        public void RegisterHotkey(Window window, ModifierKeys modifiers, System.Windows.Forms.Keys key)
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            _hotkeyId = GetHashCode();
            
            if (!RegisterHotKey(_windowHandle, _hotkeyId, (uint)modifiers, (uint)key))
            {
                throw new InvalidOperationException("Не удалось зарегистрировать горячую клавишу");
            }
        }

        public void UnregisterHotkey()
        {
            if (_windowHandle != IntPtr.Zero && _hotkeyId != 0)
            {
                UnregisterHotKey(_windowHandle, _hotkeyId);
            }

            _source?.RemoveHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                if (wParam.ToInt32() == _hotkeyId)
                {
                    HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs());
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotkey();
        }

        #region Native Methods

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion
    }

    public class HotkeyPressedEventArgs : EventArgs
    {
    }

    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
