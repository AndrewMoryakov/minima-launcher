namespace MinimalistDesktop.Services.Interfaces
{
    /// <summary>
    /// Сервис для отображения диалоговых окон (абстракция от UI)
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Показать сообщение об ошибке
        /// </summary>
        void ShowError(string message, string title);

        /// <summary>
        /// Показать информационное сообщение
        /// </summary>
        void ShowInfo(string message, string title);

        /// <summary>
        /// Показать предупреждение
        /// </summary>
        void ShowWarning(string message, string title);

        /// <summary>
        /// Показать диалог подтверждения
        /// </summary>
        /// <returns>true если пользователь подтвердил, false если отменил</returns>
        bool ShowConfirmation(string message, string title);

        /// <summary>
        /// Показать диалог добавления приложения
        /// </summary>
        /// <returns>Данные нового приложения или null если отменено</returns>
        AddAppDialogResult? ShowAddAppDialog();
    }

    /// <summary>
    /// Результат диалога добавления приложения
    /// </summary>
    public class AddAppDialogResult
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Arguments { get; set; }
    }
}
