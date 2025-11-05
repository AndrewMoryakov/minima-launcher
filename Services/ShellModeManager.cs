using System;
using System.Windows;
using MinimalistDesktop.Models;
using MinimalistDesktop.Native;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Менеджер режимов работы Shell
    /// </summary>
    public class ShellModeManager
    {
        private readonly SettingsService _settingsService;
        private AppSettings _currentSettings;
        private bool _isInitialized = false;

        public ShellModeManager(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _currentSettings = _settingsService.LoadSettings();
        }

        /// <summary>
        /// Инициализировать режим при запуске
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            ApplyMode(_currentSettings.Mode);
            _isInitialized = true;
        }

        /// <summary>
        /// Применить режим работы
        /// </summary>
        public void ApplyMode(DesktopMode mode)
        {
            _currentSettings.Mode = mode;

            switch (mode)
            {
                case DesktopMode.Overlay:
                    ApplyOverlayMode();
                    break;

                case DesktopMode.Background:
                    ApplyBackgroundMode();
                    break;

                case DesktopMode.FullReplacement:
                    ApplyFullReplacementMode();
                    break;
            }

            _settingsService.SaveSettings(_currentSettings);
        }

        /// <summary>
        /// Overlay режим - показывается по Win+Space
        /// </summary>
        private void ApplyOverlayMode()
        {
            // Восстанавливаем Explorer если нужно
            if (!Win32ShellHelper.IsExplorerRunning())
            {
                Win32ShellHelper.StartExplorer();
            }

            // Показываем панель задач
            Win32ShellHelper.ShowTaskbar();

            // Показываем иконки рабочего стола
            Win32ShellHelper.ShowDesktopIcons();
        }

        /// <summary>
        /// Background режим - всегда виден под окнами
        /// </summary>
        private void ApplyBackgroundMode()
        {
            // Explorer остается запущенным
            if (!Win32ShellHelper.IsExplorerRunning())
            {
                Win32ShellHelper.StartExplorer();
            }

            // Применяем настройки скрытия
            if (_currentSettings.HideTaskbar)
            {
                Win32ShellHelper.HideTaskbar();
            }

            if (_currentSettings.HideDesktopIcons)
            {
                Win32ShellHelper.HideDesktopIcons();
            }
        }

        /// <summary>
        /// Full Replacement режим - полная замена Shell
        /// </summary>
        private void ApplyFullReplacementMode()
        {
            var result = MessageBox.Show(
                "ВНИМАНИЕ! Вы собираетесь полностью заменить рабочий стол Windows.\n\n" +
                "Это скроет:\n" +
                "• Панель задач\n" +
                "• Иконки рабочего стола\n" +
                "• Системный трей\n\n" +
                "Для возврата нажмите Ctrl+Alt+E или перезагрузите компьютер.\n\n" +
                "Продолжить?",
                "Полная замена рабочего стола",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                _currentSettings.Mode = DesktopMode.Background;
                ApplyBackgroundMode();
                return;
            }

            // Скрываем панель задач
            Win32ShellHelper.HideTaskbar();

            // Скрываем иконки рабочего стола
            Win32ShellHelper.HideDesktopIcons();

            // Опционально завершаем Explorer
            if (_currentSettings.KillExplorer)
            {
                var confirmKill = MessageBox.Show(
                    "Завершить процесс Explorer.exe?\n\n" +
                    "Это освободит память, но некоторые функции могут не работать.\n" +
                    "Для открытия файлов используйте контекстное меню в launcher.\n\n" +
                    "Рекомендуется: НЕТ (оставить Explorer запущенным)",
                    "Завершить Explorer?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmKill == MessageBoxResult.Yes)
                {
                    Win32ShellHelper.KillExplorer();
                }
            }

            MessageBox.Show(
                "Полная замена активирована!\n\n" +
                "Горячие клавиши:\n" +
                "• Ctrl+Alt+E - восстановить Explorer\n" +
                "• Ctrl+Shift+Esc - диспетчер задач\n" +
                "• Win+L - блокировка\n" +
                "• Alt+F4 на рабочем столе - выключение",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Восстановить систему в нормальное состояние
        /// </summary>
        public void RestoreSystem()
        {
            try
            {
                // Запускаем Explorer если не запущен
                Win32ShellHelper.StartExplorer();

                // Показываем панель задач
                Win32ShellHelper.ShowTaskbar();

                // Показываем иконки рабочего стола
                Win32ShellHelper.ShowDesktopIcons();

                // Сохраняем настройки
                _currentSettings.Mode = DesktopMode.Overlay;
                _settingsService.SaveSettings(_currentSettings);

                MessageBox.Show(
                    "Система восстановлена в нормальное состояние.",
                    "Восстановление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка восстановления:\n{ex.Message}\n\n" +
                    "Попробуйте перезагрузить компьютер.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Получить текущий режим
        /// </summary>
        public DesktopMode CurrentMode => _currentSettings.Mode;

        /// <summary>
        /// Получить текущие настройки
        /// </summary>
        public AppSettings CurrentSettings => _currentSettings;

        /// <summary>
        /// Обновить настройки
        /// </summary>
        public void UpdateSettings(AppSettings settings)
        {
            _currentSettings = settings;
            _settingsService.SaveSettings(settings);
            ApplyMode(settings.Mode);
        }
    }
}
