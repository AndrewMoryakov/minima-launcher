using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MinimalistDesktop.Models;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Сервис для работы с настройками и сохранением списка приложений
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFolder;
        private readonly string _appsFilePath;
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            _settingsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MinimalistDesktop");

            _appsFilePath = Path.Combine(_settingsFolder, "apps.json");
            _settingsFilePath = Path.Combine(_settingsFolder, "settings.json");

            // Создаем папку, если её нет
            if (!Directory.Exists(_settingsFolder))
            {
                Directory.CreateDirectory(_settingsFolder);
            }
        }

        /// <summary>
        /// Загрузить список приложений
        /// </summary>
        public List<AppShortcut> LoadApps()
        {
            try
            {
                if (!File.Exists(_appsFilePath))
                    return new List<AppShortcut>();

                var json = File.ReadAllText(_appsFilePath);
                var apps = JsonSerializer.Deserialize<List<AppShortcut>>(json);
                return apps ?? new List<AppShortcut>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading apps: {ex.Message}");
                return new List<AppShortcut>();
            }
        }

        /// <summary>
        /// Сохранить список приложений
        /// </summary>
        public void SaveApps(List<AppShortcut> apps)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(apps, options);
                File.WriteAllText(_appsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving apps: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получить путь к папке настроек
        /// </summary>
        public string GetSettingsFolder() => _settingsFolder;

        /// <summary>
        /// Загрузить настройки приложения
        /// </summary>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return new AppSettings(); // Возвращаем настройки по умолчанию

                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                return new AppSettings();
            }
        }

        /// <summary>
        /// Сохранить настройки приложения
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }
    }
}
