using MinimalistDesktop.Models;

namespace MinimalistDesktop.Utilities
{
    /// <summary>
    /// Утилита для парсинга типов запуска приложений
    /// </summary>
    public static class LaunchTypeParser
    {
        /// <summary>
        /// Преобразует строковое представление типа запуска в enum
        /// </summary>
        public static LaunchType Parse(string type)
        {
            return type switch
            {
                "Standard" => LaunchType.Standard,
                "UWP" => LaunchType.UWP,
                "Url" => LaunchType.Url,
                "Command" => LaunchType.Command,
                _ => LaunchType.Standard
            };
        }
    }
}
