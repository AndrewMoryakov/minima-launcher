using System.Windows;
using Microsoft.Win32;

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
                Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*",
                Title = "Выберите приложение"
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
                MessageBox.Show("Введите название приложения", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(AppPath))
            {
                MessageBox.Show("Введите путь к приложению", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PathTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
