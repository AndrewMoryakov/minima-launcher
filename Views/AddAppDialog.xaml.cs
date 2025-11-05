using System.Windows;
using Microsoft.Win32;
using MinimalistDesktop.Constants;

namespace MinimalistDesktop.Views
{
    public partial class AddAppDialog : Window
    {
        public string AppName => NameTextBox.Text;
        public string AppPath => PathTextBox.Text;
        public string AppArguments => ArgumentsTextBox.Text;

        public AddAppDialog()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = UIConstants.ExecutableFilesFilter,
                Title = UIConstants.SelectApplication
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = openFileDialog.FileName;
                
                // Если название не заполнено, подставляем имя файла
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AppName))
            {
                MessageBox.Show(UIConstants.EnterAppName, UIConstants.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(AppPath))
            {
                MessageBox.Show(UIConstants.EnterAppPath, UIConstants.ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PathTextBox.Focus();
                return;
            }

            // Валидация пути к файлу
            var expandedPath = System.Environment.ExpandEnvironmentVariables(AppPath);

            // Проверяем только для обычных файлов (не URL и не команды)
            if (!AppPath.StartsWith("http://") &&
                !AppPath.StartsWith("https://") &&
                !IsSystemCommand(AppPath))
            {
                if (!System.IO.File.Exists(expandedPath))
                {
                    var result = MessageBox.Show(
                        string.Format(UIConstants.FileNotFoundTemplate, expandedPath),
                        UIConstants.WarningTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        PathTextBox.Focus();
                        return;
                    }
                }
            }

            DialogResult = true;
            Close();
        }

        private bool IsSystemCommand(string path)
        {
            // Проверяем, является ли команда системной
            var fileName = System.IO.Path.GetFileName(path).ToLowerInvariant();
            var systemCommands = new[]
            {
                "notepad.exe", "calc.exe", "mspaint.exe", "explorer.exe",
                "cmd.exe", "powershell.exe", "control.exe", "taskmgr.exe"
            };

            return System.Array.Exists(systemCommands, cmd => cmd == fileName);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
