using System;
using System.IO;
using MinimalistDesktop.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Сервис для работы с конфигурацией из YAML файла
    /// </summary>
    public class ConfigService
    {
        private static readonly string ConfigFileName = "config.yaml";
        private static AppConfig _cachedConfig;

        /// <summary>
        /// Загружает конфигурацию из YAML файла
        /// </summary>
        public AppConfig LoadConfig()
        {
            // Возвращаем кэшированную конфигурацию, если она уже загружена
            if (_cachedConfig != null)
                return _cachedConfig;

            try
            {
                // Путь к конфигурационному файлу (рядом с exe)
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

                if (!File.Exists(configPath))
                {
                    // Если файл не существует, создаем конфигурацию по умолчанию
                    _cachedConfig = CreateDefaultConfig();
                    SaveConfig(_cachedConfig, configPath);
                    return _cachedConfig;
                }

                // Читаем YAML файл
                var yaml = File.ReadAllText(configPath);

                // Десериализуем
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                _cachedConfig = deserializer.Deserialize<AppConfig>(yaml);
                return _cachedConfig;
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем конфигурацию по умолчанию
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки конфигурации: {ex.Message}");
                _cachedConfig = CreateDefaultConfig();
                return _cachedConfig;
            }
        }

        /// <summary>
        /// Сохраняет конфигурацию в YAML файл
        /// </summary>
        public void SaveConfig(AppConfig config, string path)
        {
            try
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(config);
                File.WriteAllText(path, yaml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения конфигурации: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает конфигурацию по умолчанию
        /// </summary>
        private AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                SearchPaths = new System.Collections.Generic.List<SearchPath>
                {
                    new SearchPath { Type = "SpecialFolder", Value = "ProgramFiles" },
                    new SearchPath { Type = "SpecialFolder", Value = "ProgramFilesX86" },
                    new SearchPath { Type = "SpecialFolder", Value = "UserStartMenu" },
                    new SearchPath { Type = "SpecialFolder", Value = "CommonStartMenu" }
                },
                SystemApps = new System.Collections.Generic.List<SystemApp>
                {
                    new SystemApp { Name = "Notepad", Command = "notepad.exe", Type = "Command" },
                    new SystemApp { Name = "Calculator", Command = "calc.exe", Type = "Command" },
                    new SystemApp { Name = "Paint", Command = "mspaint.exe", Type = "Command" },
                    new SystemApp { Name = "WordPad", Command = "write.exe", Type = "Command" },
                    new SystemApp { Name = "Task Manager", Command = "taskmgr.exe", Type = "Command" },
                    new SystemApp { Name = "Command Prompt", Command = "cmd.exe", Type = "Command" },
                    new SystemApp { Name = "PowerShell", Command = "powershell.exe", Type = "Command" },
                    new SystemApp { Name = "File Explorer", Command = "explorer.exe", Type = "Command" }
                },
                DefaultPinnedApps = new System.Collections.Generic.List<DefaultApp>
                {
                    new DefaultApp { Name = "Chrome", Path = @"C:\Program Files\Google\Chrome\Application\chrome.exe", Arguments = "", Type = "Standard", Order = 0 },
                    new DefaultApp { Name = "Visual Studio Code", Path = @"C:\Users\%USERNAME%\AppData\Local\Programs\Microsoft VS Code\Code.exe", Arguments = "", Type = "Standard", Order = 1 },
                    new DefaultApp { Name = "Notepad", Path = "notepad.exe", Arguments = "", Type = "Command", Order = 2 },
                    new DefaultApp { Name = "Calculator", Path = "calc.exe", Arguments = "", Type = "Command", Order = 3 },
                    new DefaultApp { Name = "Explorer", Path = "explorer.exe", Arguments = "", Type = "Command", Order = 4 }
                },
                ServiceFilePrefixes = new System.Collections.Generic.List<string>
                {
                    "unins", "update", "crash", "helper", "install", "setup"
                }
            };
        }

        /// <summary>
        /// Очищает кэш конфигурации
        /// </summary>
        public void ClearCache()
        {
            _cachedConfig = null;
        }
    }
}
