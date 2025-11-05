namespace MinimalistDesktop.Constants
{
    /// <summary>
    /// Константы для UI-сообщений и текстов
    /// </summary>
    public static class UIConstants
    {
        // Сообщения об ошибках
        public const string ErrorTitle = "Ошибка";
        public const string WarningTitle = "Предупреждение";
        public const string InfoTitle = "Информация";
        public const string ConfirmationTitle = "Подтверждение";

        // AddAppDialog
        public const string EnterAppName = "Введите название приложения";
        public const string EnterAppPath = "Введите путь к приложению";
        public const string FileNotFoundTemplate = "Файл не найден:\n{0}\n\nВы уверены, что хотите добавить это приложение?";
        public const string SelectApplication = "Выберите приложение";
        public const string ExecutableFilesFilter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";

        // LauncherViewModel
        public const string LaunchErrorTemplate = "Ошибка запуска приложения:\n{0}";
        public const string RemoveAppConfirmationTemplate = "Удалить '{0}' из списка?";

        // ShellModeManager
        public const string FullReplacementWarning =
            "ВНИМАНИЕ! Вы собираетесь полностью заменить рабочий стол Windows.\n\n" +
            "Это скроет:\n" +
            "• Панель задач\n" +
            "• Иконки рабочего стола\n" +
            "• Системный трей\n\n" +
            "Для возврата нажмите Ctrl+Alt+E или перезагрузите компьютер.\n\n" +
            "Продолжить?";

        public const string KillExplorerConfirmation =
            "Завершить процесс Explorer.exe?\n\n" +
            "Это освободит память, но некоторые функции могут не работать.\n" +
            "Для открытия файлов используйте контекстное меню в launcher.\n\n" +
            "Рекомендуется: НЕТ (оставить Explorer запущенным)";

        public const string FullReplacementActivated =
            "Полная замена активирована!\n\n" +
            "Горячие клавиши:\n" +
            "• Ctrl+Alt+E - восстановить Explorer\n" +
            "• Ctrl+Shift+Esc - диспетчер задач\n" +
            "• Win+L - блокировка\n" +
            "• Alt+F4 на рабочем столе - выключение";

        public const string SystemRestored = "Система восстановлена в нормальное состояние.";
        public const string RestoreTitle = "Восстановление";
        public const string RestoreErrorTemplate =
            "Ошибка восстановления:\n{0}\n\n" +
            "Попробуйте перезагрузить компьютер.";

        // App.xaml.cs
        public const string HotkeyRegistrationErrorTemplate =
            "Не удалось зарегистрировать горячую клавишу Ctrl+Shift+Space:\n{0}\n\n" +
            "Приложение будет работать, но вызывать окно можно будет только через трей.";

        public const string ModeChangeErrorTemplate = "Ошибка при смене режима:\n{0}";
        public const string AutostartEnabled = "Автозагрузка включена";
        public const string AutostartDisabled = "Автозагрузка отключена";
        public const string AutostartErrorTemplate = "Ошибка настройки автозагрузки:\n{0}";

        public const string CriticalErrorTemplate =
            "Произошла критическая ошибка. Рабочий стол Windows будет восстановлен.\n\n{0}";
        public const string ErrorOccurredTemplate =
            "Произошла ошибка. Рабочий стол Windows будет восстановлен.\n\n{0}";

        // LauncherWindow
        public const string RestoreDesktopErrorTemplate = "Ошибка при восстановлении рабочего стола:\n{0}";
    }
}
