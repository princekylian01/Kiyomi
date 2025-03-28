using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kiyomi.Updater;

namespace Kiyomi
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // По загрузке окна проверяем обновления (асинхронно)
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                bool isUpdate = await UpdateManager.IsUpdateAvailableAsync();
                if (isUpdate)
                {
                    // Показываем окно загрузки
                    LoadingWindow loadingWindow = new LoadingWindow();
                    loadingWindow.Show();

                    // Закрываем текущее окно (необязательно, на ваше усмотрение)
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке обновлений: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Перемещение окна
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика авторизации (пример)
            MessageBox.Show("Авторизация пока не реализована.");
        }

        private void LoginTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //когда нажата убирает текст login
            //когда не нажата текст login

        }
    }
}
