using System.Windows;
using MinimalistDesktop.Constants;
using MinimalistDesktop.Services.Interfaces;
using MinimalistDesktop.Views;

namespace MinimalistDesktop.Services
{
    /// <summary>
    /// Реализация сервиса диалоговых окон
    /// </summary>
    public class DialogService : IDialogService
    {
        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInfo(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public bool ShowConfirmation(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public AddAppDialogResult? ShowAddAppDialog()
        {
            var dialog = new AddAppDialog();
            if (dialog.ShowDialog() == true)
            {
                return new AddAppDialogResult
                {
                    Name = dialog.AppName,
                    Path = dialog.AppPath,
                    Arguments = dialog.AppArguments
                };
            }
            return null;
        }
    }
}
