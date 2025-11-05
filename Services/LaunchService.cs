using System;
using System.Diagnostics;
using System.IO;
using MinimalistDesktop.Models;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Сервис для запуска приложений
    /// </summary>
    public class LaunchService
    {
        public void Launch(AppShortcut app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            switch (app.Type)
            {
                case LaunchType.Standard:
                    LaunchStandardApp(app);
                    break;

                case LaunchType.Url:
                    LaunchUrl(app.Path);
                    break;

                case LaunchType.Command:
                    LaunchCommand(app);
                    break;

                case LaunchType.UWP:
                    LaunchUWPApp(app);
                    break;

                default:
                    throw new NotSupportedException($"Launch type {app.Type} is not supported");
            }
        }

        private void LaunchStandardApp(AppShortcut app)
        {
            var path = Environment.ExpandEnvironmentVariables(app.Path);

            if (!File.Exists(path) && !IsSystemCommand(path))
            {
                throw new FileNotFoundException($"Приложение не найдено: {path}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = app.Arguments ?? string.Empty,
                UseShellExecute = true
            };

            if (!string.IsNullOrWhiteSpace(app.WorkingDirectory))
            {
                startInfo.WorkingDirectory = Environment.ExpandEnvironmentVariables(app.WorkingDirectory);
            }

            Process.Start(startInfo);
        }

        private void LaunchUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void LaunchCommand(AppShortcut app)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {app.Path} {app.Arguments}",
                UseShellExecute = true,
                CreateNoWindow = false
            });
        }

        private void LaunchUWPApp(AppShortcut app)
        {
            // Для UWP приложений используется explorer.exe с shell:AppsFolder
            // Например: explorer.exe shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb3d8bbwe!App
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = app.Path,
                UseShellExecute = true
            });
        }

        private bool IsSystemCommand(string path)
        {
            // Проверяем, является ли команда системной (доступной через PATH)
            var systemCommands = new[] 
            { 
                "notepad.exe", "calc.exe", "mspaint.exe", "explorer.exe",
                "cmd.exe", "powershell.exe", "control.exe"
            };

            return Array.Exists(systemCommands, cmd => 
                cmd.Equals(Path.GetFileName(path), StringComparison.OrdinalIgnoreCase));
        }
    }
}
