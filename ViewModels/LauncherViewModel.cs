using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using MinimalistDesktop.Models;
using MinimalistDesktop.Services;
using MinimalistDesktop.Utilities;

namespace MinimalistDesktop.ViewModels
{
    public class LauncherViewModel : INotifyPropertyChanged
    {
        private readonly LaunchService _launchService;
        private readonly SettingsService _settingsService;
        private readonly AppDiscoveryService _discoveryService;
        private readonly ConfigService _configService;
        private readonly DispatcherTimer _timeTimer;

        private ObservableCollection<AppShortcut> _apps;
        private ObservableCollection<AppShortcut> _allApps;
        private ObservableCollection<AppShortcut> _filteredApps;
        private AppShortcut _selectedApp;
        private string _searchQuery;
        private string _currentTime;
        private bool _showAllApps;

        public LauncherViewModel()
        {
            _launchService = new LaunchService();
            _settingsService = new SettingsService();
            _discoveryService = new AppDiscoveryService();
            _configService = new ConfigService();

            // Загружаем закрепленные приложения (сохраненные)
            var savedApps = _settingsService.LoadApps();
            foreach (var app in savedApps)
            {
                app.IsPinned = true; // Все сохраненные = закрепленные
            }
            Apps = new ObservableCollection<AppShortcut>(savedApps);

            // Загружаем все системные приложения в фоне
            LoadAllApps();

            // По умолчанию показываем только закрепленные
            _showAllApps = false;
            FilteredApps = new ObservableCollection<AppShortcut>(Apps);

            // Таймер для отображения времени
            _timeTimer = new DispatcherTimer();
            _timeTimer.Interval = TimeSpan.FromSeconds(1);
            _timeTimer.Tick += (s, e) => UpdateTime();
            _timeTimer.Start();
            UpdateTime();

            // Если нет приложений, добавляем примеры
            if (Apps.Count == 0)
            {
                AddDefaultApps();
            }
        }

        private void LoadAllApps()
        {
            // Загружаем асинхронно в фоне
            System.Threading.Tasks.Task.Run(() =>
            {
                var discoveredApps = _discoveryService.DiscoverApplications();

                // Возвращаемся в UI поток
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllApps = new ObservableCollection<AppShortcut>(discoveredApps);
                });
            });
        }

        #region Properties

        public ObservableCollection<AppShortcut> Apps
        {
            get => _apps;
            set
            {
                _apps = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AppShortcut> AllApps
        {
            get => _allApps;
            set
            {
                _allApps = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAllApps
        {
            get => _showAllApps;
            set
            {
                _showAllApps = value;
                OnPropertyChanged();
                FilterApps();
            }
        }

        public ObservableCollection<AppShortcut> FilteredApps
        {
            get => _filteredApps;
            set
            {
                _filteredApps = value;
                OnPropertyChanged();
                
                // Автоматически выбираем первый элемент
                if (_filteredApps.Count > 0)
                {
                    SelectedApp = _filteredApps[0];
                }
            }
        }

        public AppShortcut SelectedApp
        {
            get => _selectedApp;
            set
            {
                _selectedApp = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                FilterApps();
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Events

        public event EventHandler CloseRequested;
        public event EventHandler AppLaunched;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        public void LaunchSelectedApp()
        {
            if (SelectedApp == null) return;

            try
            {
                _launchService.Launch(SelectedApp);
                AppLaunched?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска приложения:\n{ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowAddAppDialog()
        {
            var dialog = new Views.AddAppDialog();
            if (dialog.ShowDialog() == true)
            {
                var newApp = new AppShortcut
                {
                    Name = dialog.AppName,
                    Path = dialog.AppPath,
                    Arguments = dialog.AppArguments,
                    Order = Apps.Count
                };

                Apps.Add(newApp);
                FilteredApps.Add(newApp);
                SaveApps();
            }
        }

        public void RemoveSelectedApp()
        {
            if (SelectedApp == null) return;

            var result = MessageBox.Show(
                $"Удалить '{SelectedApp.Name}' из списка?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Apps.Remove(SelectedApp);
                FilterApps();
                SaveApps();
            }
        }

        private void FilterApps()
        {
            var sourceApps = GetSourceApps();

            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredApps = new ObservableCollection<AppShortcut>(sourceApps);
            }
            else
            {
                var query = SearchQuery.ToLower();
                var filtered = sourceApps.Where(app =>
                    app.Name.ToLower().Contains(query)).ToList();
                FilteredApps = new ObservableCollection<AppShortcut>(filtered);
            }
        }

        private System.Collections.Generic.List<AppShortcut> GetSourceApps()
        {
            if (!ShowAllApps)
            {
                // Показываем только закрепленные
                return Apps.ToList();
            }
            else
            {
                // Объединяем: закрепленные (Apps) + все остальные (AllApps)
                var combined = new System.Collections.Generic.List<AppShortcut>();

                // Сначала закрепленные
                combined.AddRange(Apps);

                // Затем все остальные приложения (которые не закреплены)
                if (AllApps != null)
                {
                    var pinnedPaths = new HashSet<string>(Apps.Select(a => a.Path), StringComparer.OrdinalIgnoreCase);
                    var unpinnedApps = AllApps.Where(a => !pinnedPaths.Contains(a.Path));
                    combined.AddRange(unpinnedApps);
                }

                return combined;
            }
        }

        public void ToggleShowAllApps()
        {
            ShowAllApps = !ShowAllApps;
        }

        private void SaveApps()
        {
            _settingsService.SaveApps(Apps.ToList());
        }

        private void UpdateTime()
        {
            CurrentTime = DateTime.Now.ToString("HH:mm");
        }

        private void AddDefaultApps()
        {
            var config = _configService.LoadConfig();

            // Загружаем приложения по умолчанию из конфигурации
            foreach (var defaultApp in config.DefaultPinnedApps)
            {
                // Раскрываем переменные окружения в пути
                var expandedPath = Environment.ExpandEnvironmentVariables(defaultApp.Path);

                // Проверяем существование только для обычных приложений
                if (defaultApp.Type == "Standard" && !File.Exists(expandedPath))
                {
                    // Пропускаем, если файл не найден
                    continue;
                }

                var app = new AppShortcut
                {
                    Name = defaultApp.Name,
                    Path = expandedPath,
                    Arguments = defaultApp.Arguments ?? string.Empty,
                    Order = defaultApp.Order,
                    Type = LaunchTypeParser.Parse(defaultApp.Type),
                    IsPinned = true
                };

                Apps.Add(app);
            }

            FilteredApps = new ObservableCollection<AppShortcut>(Apps);
            SaveApps();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
