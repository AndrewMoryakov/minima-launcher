using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MinimalistDesktop.Native;
using MinimalistDesktop.Services;
using MinimalistDesktop.ViewModels;

namespace MinimalistDesktop.Views
{
    public partial class LauncherWindow : Window
    {
        private const double CollapsedListMaxHeight = 300;
        private readonly LauncherViewModel _viewModel;
        private readonly ShellModeManager _shellModeManager;
        private bool _suppressSearchFocusHandler;

        private double _expandedListMaxHeight;
        public LauncherWindow()
        {
            InitializeComponent();
            _viewModel = new LauncherViewModel();
            DataContext = _viewModel;

            // Инициализация ShellModeManager
            _shellModeManager = new ShellModeManager(new SettingsService());

            // Подписка на события ViewModel
            _viewModel.CloseRequested += OnCloseRequested;
            _viewModel.AppLaunched += OnAppLaunched;

            // Подписываемся на SourceInitialized для получения HWND
            this.SourceInitialized += LauncherWindow_SourceInitialized;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            AppListContainer.SizeChanged += AppListContainer_SizeChanged;
            ToolbarPanel.SizeChanged += ToolbarPanel_SizeChanged;
        }

        private void LauncherWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Устанавливаем окно на desktop level (позади всех окон)
            SetWindowToDesktopLevel();
            UpdateExpandedListTarget();
        }

        private void SetWindowToDesktopLevel()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                // Помещаем окно в самый низ Z-order (как рабочий стол)
                // SWP_NOACTIVATE - не активировать окно
                // SWP_NOMOVE - не двигать
                // SWP_NOSIZE - не изменять размер
                Win32ShellHelper.SetWindowPos(
                    hwnd,
                    Win32ShellHelper.HWND_BOTTOM,
                    0, 0, 0, 0,
                    Win32ShellHelper.SWP_NOMOVE |
                    Win32ShellHelper.SWP_NOSIZE |
                    Win32ShellHelper.SWP_NOACTIVATE);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.GotFocus += SearchBox_GotFocus;
            SearchBox.PreviewMouseLeftButtonDown += SearchBox_PreviewMouseLeftButtonDown;

            AppListBox.MaxHeight = CollapsedListMaxHeight;
            // Ignore programmatic focus so expansion reacts to user input.
            _suppressSearchFocusHandler = true;
            SearchBox.Focus();

            SetWindowToDesktopLevel();
            UpdateExpandedListTarget();

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            this.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_suppressSearchFocusHandler)
            {
                _suppressSearchFocusHandler = false;
                return;
            }

            EnsureAllAppsShown();
        }

        private void SearchBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnsureAllAppsShown();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Возвращаем окно на desktop level при активации
            SetWindowToDesktopLevel();
            UpdateExpandedListTarget();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Окно остается на desktop level
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_viewModel.ShowAllApps)
            {
                return;
            }

            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.OriginalSource is not DependencyObject source)
            {
                _viewModel.ShowAllApps = false;
                return;
            }

            if (IsInsideInteractiveElements(source))
            {
                return;
            }

            _viewModel.ShowAllApps = false;
        }

        private bool IsInsideInteractiveElements(DependencyObject source)
        {
            while (source != null)
            {
                if (ReferenceEquals(source, SearchBox) || ReferenceEquals(source, AppListBox))
                {
                    return true;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private void AppListContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateExpandedListTarget();
        }

        private void ToolbarPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateExpandedListTarget();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(LauncherViewModel.ShowAllApps))
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var expanding = _viewModel.ShowAllApps;
                var target = expanding ? GetExpandedListHeight() : CollapsedListMaxHeight;
                AnimateAppListHeight(target, expanding);
            }));
        }

        private double GetExpandedListHeight()
        {
            if (_expandedListMaxHeight <= CollapsedListMaxHeight)
            {
                UpdateExpandedListTarget();
            }

            return Math.Max(_expandedListMaxHeight, CollapsedListMaxHeight);
        }

        private void AnimateAppListHeight(double targetHeight, bool expanding)
        {
            AppListBox.BeginAnimation(FrameworkElement.MaxHeightProperty, null);

            var currentHeight = AppListBox.MaxHeight;
            if (double.IsNaN(currentHeight) || double.IsInfinity(currentHeight) || currentHeight <= 0)
            {
                currentHeight = AppListBox.ActualHeight > 0 ? AppListBox.ActualHeight : CollapsedListMaxHeight;
            }

            var animation = new DoubleAnimation
            {
                From = currentHeight,
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(expanding ? 400 : 300),
                EasingFunction = new CubicEase { EasingMode = expanding ? EasingMode.EaseOut : EasingMode.EaseIn }
            };

            animation.Completed += (_, _) => AppListBox.MaxHeight = targetHeight;
            AppListBox.BeginAnimation(FrameworkElement.MaxHeightProperty, animation);
        }

        private void UpdateExpandedListTarget()
        {
            var containerHeight = AppListContainer.ActualHeight;
            if (containerHeight <= 0)
            {
                return;
            }

            var toolbarHeight = ToolbarPanel?.ActualHeight ?? 0;
            var toolbarTopOffset = ToolbarPanel?.Margin.Top ?? 0;
            var availableHeight = containerHeight - (toolbarHeight + toolbarTopOffset);
            if (availableHeight < CollapsedListMaxHeight)
            {
                availableHeight = CollapsedListMaxHeight;
            }

            _expandedListMaxHeight = availableHeight;

            if (_viewModel.ShowAllApps)
            {
                AppListBox.MaxHeight = _expandedListMaxHeight;
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Перехватываем стрелки ДО того, как их обработает TextBox
            switch (e.Key)
            {
                case Key.Down:
                    if (AppListBox.Items.Count > 0)
                    {
                        if (AppListBox.SelectedIndex < AppListBox.Items.Count - 1)
                            AppListBox.SelectedIndex++;
                        else if (AppListBox.SelectedIndex == -1)
                            AppListBox.SelectedIndex = 0;

                        AppListBox.ScrollIntoView(AppListBox.SelectedItem);
                        e.Handled = true;
                    }
                    break;

                case Key.Up:
                    if (AppListBox.Items.Count > 0)
                    {
                        if (AppListBox.SelectedIndex > 0)
                            AppListBox.SelectedIndex--;

                        AppListBox.ScrollIntoView(AppListBox.SelectedItem);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    HideWindow();
                    e.Handled = true;
                    break;

                case Key.Enter:
                    _viewModel.LaunchSelectedApp();
                    e.Handled = true;
                    break;

                case Key.N:
                    if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    {
                        _viewModel.ShowAddAppDialog();
                        e.Handled = true;
                    }
                    break;

                case Key.Delete:
                    if (_viewModel.SelectedApp != null)
                    {
                        _viewModel.RemoveSelectedApp();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            HideWindow();
        }

        private void OnAppLaunched(object sender, EventArgs e)
        {
            // Скрываем окно после запуска приложения
            HideWindow();
        }

        private void HideWindow()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, e) => this.Hide();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        public void ShowWindow()
        {
            this.Show();
            this.Opacity = 0;
            this.Activate();
            SearchBox.Clear();
            _viewModel.ShowAllApps = false;
            _suppressSearchFocusHandler = true;
            SearchBox.Focus();

            // Устанавливаем окно на desktop level
            SetWindowToDesktopLevel();
            UpdateExpandedListTarget();

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            this.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void RestoreDesktopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _shellModeManager.RestoreSystem();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при восстановлении рабочего стола:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EnsureAllAppsShown()
        {
            if (!_viewModel.ShowAllApps)
            {
                _viewModel.ShowAllApps = true;
            }
        }

    }
}
