using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinimalistDesktop.Models;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Сервис для поиска установленных приложений в системе
    /// </summary>
    public class AppDiscoveryService
    {
        private readonly List<string> _searchPaths = new List<string>();
        private readonly List<string> _serviceFilePrefixes = new List<string>();
        private readonly ConfigService _configService;

        public AppDiscoveryService()
        {
            _configService = new ConfigService();
            InitializeFromConfig();
        }

        private void InitializeFromConfig()
        {
            var config = _configService.LoadConfig();

            // Инициализируем пути поиска из конфигурации
            foreach (var searchPath in config.SearchPaths)
            {
                string resolvedPath = null;

                if (searchPath.Type == "SpecialFolder")
                {
                    resolvedPath = ResolveSpecialFolder(searchPath.Value);
                }
                else if (searchPath.Type == "Custom")
                {
                    resolvedPath = Environment.ExpandEnvironmentVariables(searchPath.Value);
                }

                if (!string.IsNullOrEmpty(resolvedPath) && Directory.Exists(resolvedPath))
                {
                    _searchPaths.Add(resolvedPath);
                }
            }

            // Загружаем паттерны служебных файлов
            _serviceFilePrefixes.AddRange(config.ServiceFilePrefixes);
        }

        private string ResolveSpecialFolder(string folderName)
        {
            switch (folderName)
            {
                case "ProgramFiles":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                case "ProgramFilesX86":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                case "UserStartMenu":
                    return Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        @"Microsoft\Windows\Start Menu\Programs");
                case "CommonStartMenu":
                    return Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        @"Microsoft\Windows\Start Menu\Programs");
                default:
                    return null;
            }
        }

        /// <summary>
        /// Поиск всех установленных приложений
        /// </summary>
        public List<AppShortcut> DiscoverApplications()
        {
            var apps = new HashSet<AppShortcut>(new AppShortcutComparer());

            // Сканируем Program Files
            foreach (var path in _searchPaths)
            {
                if (Directory.Exists(path))
                {
                    var exeFiles = FindExecutables(path);
                    foreach (var app in exeFiles)
                    {
                        apps.Add(app);
                    }
                }
            }

            // Добавляем стандартные системные приложения
            AddCommonApps(apps);

            return apps.OrderBy(a => a.Name).ToList();
        }

        private List<AppShortcut> FindExecutables(string directory)
        {
            var apps = new List<AppShortcut>();

            try
            {
                // Поиск .exe файлов (не глубже 2 уровней для производительности)
                var directories = Directory.GetDirectories(directory);

                foreach (var dir in directories)
                {
                    try
                    {
                        var exeFiles = Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly);

                        foreach (var exeFile in exeFiles)
                        {
                            try
                            {
                                var fileName = Path.GetFileNameWithoutExtension(exeFile);

                                // Пропускаем служебные файлы
                                if (IsServiceFile(fileName))
                                    continue;

                                apps.Add(new AppShortcut
                                {
                                    Id = Guid.NewGuid(),
                                    Name = fileName,
                                    Path = exeFile,
                                    Arguments = string.Empty,
                                    WorkingDirectory = Path.GetDirectoryName(exeFile) ?? string.Empty,
                                    Type = LaunchType.Standard,
                                    IsPinned = false
                                });
                            }
                            catch
                            {
                                // Игнорируем ошибки доступа к файлам
                            }
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки доступа к директориям
                    }
                }

                // Поиск .lnk файлов (ярлыков из Start Menu)
                try
                {
                    var lnkFiles = Directory.GetFiles(directory, "*.lnk", SearchOption.AllDirectories);

                    foreach (var lnkFile in lnkFiles)
                    {
                        try
                        {
                            var targetPath = GetShortcutTarget(lnkFile);

                            if (!string.IsNullOrEmpty(targetPath) &&
                                File.Exists(targetPath) &&
                                targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                var fileName = Path.GetFileNameWithoutExtension(lnkFile);

                                // Пропускаем служебные файлы
                                if (IsServiceFile(fileName))
                                    continue;

                                apps.Add(new AppShortcut
                                {
                                    Id = Guid.NewGuid(),
                                    Name = fileName,
                                    Path = targetPath,
                                    Arguments = string.Empty,
                                    WorkingDirectory = Path.GetDirectoryName(targetPath) ?? string.Empty,
                                    Type = LaunchType.Standard,
                                    IsPinned = false
                                });
                            }
                        }
                        catch
                        {
                            // Игнорируем ошибки обработки ярлыков
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибки поиска .lnk файлов
                }
            }
            catch
            {
                // Игнорируем ошибки доступа
            }

            return apps;
        }

        private string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                // Используем IWshRuntimeLibrary для чтения .lnk файлов
                // Создаем через позднее связывание, чтобы избежать зависимости от COM reference
                var shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                var shortcut = shell.GetType().InvokeMember("CreateShortcut",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, shell, new object[] { shortcutPath });

                var targetPath = shortcut.GetType().InvokeMember("TargetPath",
                    System.Reflection.BindingFlags.GetProperty,
                    null, shortcut, null) as string;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(shortcut);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);

                return targetPath ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsServiceFile(string fileName)
        {
            var lowerName = fileName.ToLowerInvariant();
            return _serviceFilePrefixes.Any(prefix => lowerName.Contains(prefix));
        }

        private void AddCommonApps(HashSet<AppShortcut> apps)
        {
            var config = _configService.LoadConfig();

            // Добавляем системные приложения из конфигурации
            foreach (var systemApp in config.SystemApps)
            {
                apps.Add(new AppShortcut
                {
                    Id = Guid.NewGuid(),
                    Name = systemApp.Name,
                    Path = systemApp.Command,
                    Arguments = string.Empty,
                    WorkingDirectory = string.Empty,
                    Type = ParseLaunchType(systemApp.Type),
                    IsPinned = false
                });
            }
        }

        private LaunchType ParseLaunchType(string type)
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

        /// <summary>
        /// Компаратор для удаления дубликатов по пути
        /// </summary>
        private class AppShortcutComparer : IEqualityComparer<AppShortcut>
        {
            public bool Equals(AppShortcut? x, AppShortcut? y)
            {
                if (x == null || y == null) return false;
                return string.Equals(x.Path, y.Path, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(AppShortcut obj)
            {
                return obj.Path?.ToLowerInvariant().GetHashCode() ?? 0;
            }
        }
    }
}
