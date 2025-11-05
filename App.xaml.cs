using System;
using System.Windows;
using System.Windows.Forms;
using MinimalistDesktop.Models;
using MinimalistDesktop.Services;
using MinimalistDesktop.Views;
using Application = System.Windows.Application;

namespace MinimalistDesktop
{
    public partial class App : Application
    {
        private LauncherWindow _launcherWindow;
        private HotkeyService _hotkeyService;
        private HotkeyService _restoreHotkeyService;
        private NotifyIcon _notifyIcon;
        private SettingsService _settingsService;
        private ShellModeManager _shellModeManager;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Настраиваем обработчики необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Инициализируем сервисы
            _settingsService = new SettingsService();
            _shellModeManager = new ShellModeManager(_settingsService);

            // Создаем главное окно
            _launcherWindow = new LauncherWindow();

            // Настраиваем системный трей
            SetupSystemTray();

            // Инициализируем режим работы
            _shellModeManager.Initialize();

            // Показываем окно один раз, чтобы инициализировать HWND
            _launcherWindow.Show();
            _launcherWindow.Hide();

            // Регистрируем глобальную горячую клавишу (Ctrl + Shift + Space)
            try
            {
                _hotkeyService = new HotkeyService();
                _hotkeyService.HotkeyPressed += OnHotkeyPressed;

                // Регистрируем Ctrl + Shift + Space
                _hotkeyService.RegisterHotkey(
                    _launcherWindow,
                    ModifierKeys.Control | ModifierKeys.Shift,
                    Keys.Space);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Не удалось зарегистрировать горячую клавишу Ctrl+Shift+Space:\n{ex.Message}\n\n" +
                    "Приложение будет работать, но вызывать окно можно будет только через трей.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Регистрируем горячую клавишу для восстановления системы (Ctrl + Alt + E)
            try
            {
                _restoreHotkeyService = new HotkeyService();
                _restoreHotkeyService.HotkeyPressed += OnRestoreHotkeyPressed;

                // Регистрируем Ctrl + Alt + E
                _restoreHotkeyService.RegisterHotkey(
                    _launcherWindow,
                    ModifierKeys.Control | ModifierKeys.Alt,
                    Keys.E);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register restore hotkey: {ex.Message}");
            }

            // Показываем окно сразу при запуске
            ShowLauncher();
        }

        private void SetupSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Minimalist Desktop"
            };

            // Контекстное меню
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add("Открыть (Ctrl+Shift+Space)", null, (s, e) => ShowLauncher());
            contextMenu.Items.Add("-");

            // Подменю "Режим работы"
            var modeMenu = new ToolStripMenuItem("Режим работы");

            var overlayMode = new ToolStripMenuItem("Overlay (по умолчанию)", null, (s, e) => SetMode(DesktopMode.Overlay))
            {
                Checked = _shellModeManager.CurrentMode == DesktopMode.Overlay
            };
            modeMenu.DropDownItems.Add(overlayMode);

            var backgroundMode = new ToolStripMenuItem("Background (всегда виден)", null, (s, e) => SetMode(DesktopMode.Background))
            {
                Checked = _shellModeManager.CurrentMode == DesktopMode.Background
            };
            modeMenu.DropDownItems.Add(backgroundMode);

            var fullReplacementMode = new ToolStripMenuItem("Full Replacement (замена Shell)", null, (s, e) => SetMode(DesktopMode.FullReplacement))
            {
                Checked = _shellModeManager.CurrentMode == DesktopMode.FullReplacement
            };
            modeMenu.DropDownItems.Add(fullReplacementMode);

            modeMenu.DropDownItems.Add("-");
            modeMenu.DropDownItems.Add("Восстановить систему (Ctrl+Alt+E)", null, (s, e) => RestoreSystem());

            contextMenu.Items.Add(modeMenu);
            contextMenu.Items.Add("-");

            contextMenu.Items.Add("Настройки", null, (s, e) => OpenSettings());

            var autostartItem = new System.Windows.Forms.ToolStripMenuItem("Автозагрузка", null, (s, e) => ToggleAutostart())
            {
                CheckOnClick = true,
                Checked = IsAutostartEnabled()
            };
            contextMenu.Items.Add(autostartItem);

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Выход", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowLauncher();
        }

        private void OnHotkeyPressed(object sender, HotkeyPressedEventArgs e)
        {
            ShowLauncher();
        }

        private void ShowLauncher()
        {
            _launcherWindow.ShowWindow();
        }

        private void OpenSettings()
        {
            var settingsPath = _settingsService.GetSettingsFolder();
            System.Diagnostics.Process.Start("explorer.exe", settingsPath);
        }

        private void SetMode(DesktopMode mode)
        {
            try
            {
                _shellModeManager.ApplyMode(mode);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка при смене режима:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RestoreSystem()
        {
            _shellModeManager.RestoreSystem();
        }

        private void OnRestoreHotkeyPressed(object sender, HotkeyPressedEventArgs e)
        {
            RestoreSystem();
        }

        private bool IsAutostartEnabled()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("MinimalistDesktop") != null;
            }
            catch
            {
                return false;
            }
        }

        private void ToggleAutostart()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (key == null) return;

                if (IsAutostartEnabled())
                {
                    key.DeleteValue("MinimalistDesktop", false);
                    System.Windows.MessageBox.Show("Автозагрузка отключена", "Информация");
                }
                else
                {
                    var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    exePath = exePath.Replace(".dll", ".exe"); // Для .NET Core/5+
                    key.SetValue("MinimalistDesktop", $"\"{exePath}\"");
                    System.Windows.MessageBox.Show("Автозагрузка включена", "Информация");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка настройки автозагрузки:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExitApplication()
        {
            _hotkeyService?.Dispose();
            _restoreHotkeyService?.Dispose();
            _notifyIcon?.Dispose();
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Автоматически восстанавливаем рабочий стол при закрытии
            try
            {
                _shellModeManager?.RestoreSystem();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring desktop on exit: {ex.Message}");
            }

            _hotkeyService?.Dispose();
            _restoreHotkeyService?.Dispose();
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // Автоматически восстанавливаем рабочий стол при необработанной ошибке
                _shellModeManager?.RestoreSystem();

                var exception = e.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"Unhandled exception: {exception?.Message}");

                System.Windows.MessageBox.Show(
                    $"Произошла критическая ошибка. Рабочий стол Windows будет восстановлен.\n\n{exception?.Message}",
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in unhandled exception handler: {ex.Message}");
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                // Автоматически восстанавливаем рабочий стол при необработанной ошибке UI
                _shellModeManager?.RestoreSystem();

                System.Diagnostics.Debug.WriteLine($"Dispatcher unhandled exception: {e.Exception.Message}");

                System.Windows.MessageBox.Show(
                    $"Произошла ошибка. Рабочий стол Windows будет восстановлен.\n\n{e.Exception.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Предотвращаем закрытие приложения
                e.Handled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in dispatcher exception handler: {ex.Message}");
                e.Handled = false;
            }
        }
    }
}
