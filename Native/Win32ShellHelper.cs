using System;
using System.Runtime.InteropServices;

namespace MinimalistDesktop.Native
{
    /// <summary>
    /// Расширенный Win32Helper для полной замены Shell
    /// </summary>
    public static class Win32ShellHelper
    {
        #region Shell Integration

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, 
            string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // ShowWindow constants
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        // SetWindowPos constants
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;

        #endregion

        #region Taskbar Management

        /// <summary>
        /// Скрыть панель задач Windows
        /// </summary>
        public static void HideTaskbar()
        {
            IntPtr taskBar = FindWindow("Shell_TrayWnd", null);
            if (taskBar != IntPtr.Zero)
            {
                ShowWindow(taskBar, SW_HIDE);
            }

            // Скрыть дополнительные панели на других мониторах
            IntPtr secondaryTaskbar = FindWindow("Shell_SecondaryTrayWnd", null);
            if (secondaryTaskbar != IntPtr.Zero)
            {
                ShowWindow(secondaryTaskbar, SW_HIDE);
            }
        }

        /// <summary>
        /// Показать панель задач Windows
        /// </summary>
        public static void ShowTaskbar()
        {
            IntPtr taskBar = FindWindow("Shell_TrayWnd", null);
            if (taskBar != IntPtr.Zero)
            {
                ShowWindow(taskBar, SW_SHOW);
            }

            IntPtr secondaryTaskbar = FindWindow("Shell_SecondaryTrayWnd", null);
            if (secondaryTaskbar != IntPtr.Zero)
            {
                ShowWindow(secondaryTaskbar, SW_SHOW);
            }
        }

        #endregion

        #region Desktop Icons Management

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_COMMAND = 0x0111;
        private const uint TOGGLE_DESKTOP = 0x7402;

        /// <summary>
        /// Скрыть иконки рабочего стола
        /// </summary>
        public static void HideDesktopIcons()
        {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;

            // Найти WorkerW окно
            EnumWindows((hwnd, lParam) =>
            {
                IntPtr shellView = FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView != IntPtr.Zero)
                {
                    result = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (result != IntPtr.Zero)
            {
                ShowWindow(result, SW_HIDE);
            }

            // Также скрываем Progman
            if (progman != IntPtr.Zero)
            {
                ShowWindow(progman, SW_HIDE);
            }
        }

        /// <summary>
        /// Показать иконки рабочего стола
        /// </summary>
        public static void ShowDesktopIcons()
        {
            IntPtr progman = FindWindow("Progman", null);
            if (progman != IntPtr.Zero)
            {
                ShowWindow(progman, SW_SHOW);
            }

            IntPtr result = IntPtr.Zero;
            EnumWindows((hwnd, lParam) =>
            {
                IntPtr shellView = FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView != IntPtr.Zero)
                {
                    result = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (result != IntPtr.Zero)
            {
                ShowWindow(result, SW_SHOW);
            }
        }

        #endregion

        #region Explorer Management

        /// <summary>
        /// Проверить, запущен ли Explorer.exe
        /// </summary>
        public static bool IsExplorerRunning()
        {
            return System.Diagnostics.Process.GetProcessesByName("explorer").Length > 0;
        }

        /// <summary>
        /// Запустить Explorer.exe
        /// </summary>
        public static void StartExplorer()
        {
            if (!IsExplorerRunning())
            {
                System.Diagnostics.Process.Start("explorer.exe");
            }
        }

        /// <summary>
        /// Завершить Explorer.exe
        /// </summary>
        public static void KillExplorer()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName("explorer");
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        #endregion

        #region Window Management

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public const int SW_RESTORE = 9;
        public const int SW_MINIMIZE = 6;

        /// <summary>
        /// Получить все видимые окна
        /// </summary>
        public static System.Collections.Generic.List<WindowInfo> GetAllWindows()
        {
            var windows = new System.Collections.Generic.List<WindowInfo>();

            EnumWindows((hwnd, lParam) =>
            {
                if (IsWindowVisible(hwnd))
                {
                    var title = new System.Text.StringBuilder(256);
                    GetWindowText(hwnd, title, 256);

                    if (!string.IsNullOrWhiteSpace(title.ToString()))
                    {
                        windows.Add(new WindowInfo
                        {
                            Handle = hwnd,
                            Title = title.ToString(),
                            IsMinimized = IsIconic(hwnd)
                        });
                    }
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        #endregion
    }

    /// <summary>
    /// Информация об окне
    /// </summary>
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }
        public bool IsMinimized { get; set; }
    }
}
