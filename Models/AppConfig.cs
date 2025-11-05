using System.Collections.Generic;

namespace MinimalistDesktop.Models
{
    /// <summary>
    /// Конфигурация приложения из YAML файла
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Источники для поиска программ
        /// </summary>
        public List<SearchPath> SearchPaths { get; set; } = new List<SearchPath>();

        /// <summary>
        /// Системные приложения
        /// </summary>
        public List<SystemApp> SystemApps { get; set; } = new List<SystemApp>();

        /// <summary>
        /// Программы по умолчанию (закрепленные при первом запуске)
        /// </summary>
        public List<DefaultApp> DefaultPinnedApps { get; set; } = new List<DefaultApp>();

        /// <summary>
        /// Паттерны для фильтрации служебных файлов
        /// </summary>
        public List<string> ServiceFilePrefixes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Путь поиска программ
    /// </summary>
    public class SearchPath
    {
        /// <summary>
        /// Тип пути: SpecialFolder или Custom
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Значение (имя SpecialFolder или путь)
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Системное приложение
    /// </summary>
    public class SystemApp
    {
        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Команда запуска
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Тип приложения
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Приложение по умолчанию
    /// </summary>
    public class DefaultApp
    {
        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Путь к исполняемому файлу
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Аргументы командной строки
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Тип приложения
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Порядок отображения
        /// </summary>
        public int Order { get; set; }
    }
}
