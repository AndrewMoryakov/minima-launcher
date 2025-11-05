using System;

namespace MinimalistDesktop.Models
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Режим работы рабочего стола
        /// </summary>
        public DesktopMode Mode { get; set; } = DesktopMode.Overlay;

        /// <summary>
        /// Скрывать панель задач Windows
        /// </summary>
        public bool HideTaskbar { get; set; } = false;

        /// <summary>
        /// Скрывать иконки рабочего стола Windows
        /// </summary>
        public bool HideDesktopIcons { get; set; } = false;

        /// <summary>
        /// Завершать процесс Explorer.exe
        /// </summary>
        public bool KillExplorer { get; set; } = false;

        /// <summary>
        /// Фоновое изображение
        /// </summary>
        public string? BackgroundImage { get; set; }

        /// <summary>
        /// Цвет фона
        /// </summary>
        public string BackgroundColor { get; set; } = "#000000";

        /// <summary>
        /// Прозрачность фона (0.0 - 1.0)
        /// </summary>
        public double BackgroundOpacity { get; set; } = 0.95;

        /// <summary>
        /// Радиус размытия для фона
        /// </summary>
        public double BlurRadius { get; set; } = 0;

        /// <summary>
        /// Показывать часы
        /// </summary>
        public bool ShowClock { get; set; } = true;

        /// <summary>
        /// Показывать дату
        /// </summary>
        public bool ShowDate { get; set; } = true;

        /// <summary>
        /// Размер шрифта для элементов списка
        /// </summary>
        public int FontSize { get; set; } = 24;

        /// <summary>
        /// Семейство шрифта
        /// </summary>
        public string FontFamily { get; set; } = "Segoe UI Light";

        /// <summary>
        /// Автозапуск с Windows
        /// </summary>
        public bool Autostart { get; set; } = false;
    }

    /// <summary>
    /// Режим работы рабочего стола
    /// </summary>
    public enum DesktopMode
    {
        /// <summary>
        /// Оверлей - показывается по Win+Space, скрывается после запуска
        /// </summary>
        Overlay,

        /// <summary>
        /// Фоновый режим - всегда виден под окнами приложений
        /// </summary>
        Background,

        /// <summary>
        /// Полная замена - заменяет Explorer.exe как Shell
        /// </summary>
        FullReplacement
    }
}
