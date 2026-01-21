using System.Configuration;
using System.Data;
using System.Windows;

namespace AppGlobus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Статическое свойство для хранения данных текущего пользователя
        public static UserInfo? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // При запуске показываем окно авторизации
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
