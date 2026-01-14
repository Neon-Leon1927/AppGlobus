using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppGlobus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Правильные логин и пароль (для примера)
        private const string CorrectLogin = "admin";
        private const string CorrectPassword = "12345";

        public MainWindow()
        {
            InitializeComponent();
        }
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }
        private void TextBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }
        // Метод проверки логина и пароля
        private void TryLogin()
        {
            string login = TextBoxLogin.Text;
            string password = TextBoxPassword.Text;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowErrorMessage("Введите логин и пароль");
                return;
            }

            // Проверка логина и пароля
            if (login == CorrectLogin && password == CorrectPassword)
            {
                // Успешный вход
                MessageBox.Show("Вход выполнен успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Здесь можно открыть главное окно приложения
                // MainMenuWindow mainMenu = new MainMenuWindow();
                // mainMenu.Show();
                // this.Close();
            }
            else
            {
                ShowErrorMessage("Неверный логин или пароль");

                // Очищаем поле пароля при ошибке
                TextBoxPassword.Clear();
                TextBoxPassword.Focus();
            }
        }
        // Метод для отображения ошибки
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка входа",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        // Эффект при наведении курсора на кнопку
        private void ButtonLogin_MouseEnter(object sender, MouseEventArgs e)
        {
            // Изменяем цвет кнопки при наведении
            ButtonLogin.Background = new RadialGradientBrush(
                Color.FromRgb(100, 230, 255), // Более светлый цвет
                Color.FromRgb(100, 180, 255));

            // Можно также изменить курсор
            ButtonLogin.Cursor = Cursors.Hand;

            // Небольшое увеличение кнопки (опционально)
            ButtonLogin.FontSize = 13;
        }        
    }
}