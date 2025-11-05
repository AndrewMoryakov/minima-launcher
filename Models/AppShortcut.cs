using System;

namespace MinimalistDesktop.Models
{
    /// <summary>
    /// Модель ярлыка приложения
    /// </summary>
    public class AppShortcut
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Отображаемое имя приложения
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Путь к исполняемому файлу или команде
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Аргументы командной строки (опционально)
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Рабочая директория (опционально)
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Порядок отображения
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Тип запуска
        /// </summary>
        public LaunchType Type { get; set; } = LaunchType.Standard;

        /// <summary>
        /// Закреплено ли приложение
        /// </summary>
        public bool IsPinned { get; set; } = false;

        public override string ToString() => Name;
    }

    public enum LaunchType
    {
        /// <summary>
        /// Обычное приложение
        /// </summary>
        Standard,

        /// <summary>
        /// UWP приложение (Microsoft Store)
        /// </summary>
        UWP,

        /// <summary>
        /// URL (браузер)
        /// </summary>
        Url,

        /// <summary>
        /// Системная команда
        /// </summary>
        Command
    }
}
