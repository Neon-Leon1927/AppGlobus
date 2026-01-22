using Microsoft.Data.SqlClient;
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
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT Роль, ФИО 
                        FROM Users 
                        WHERE Логин = @login AND Пароль = @password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string? role = reader["Роль"]?.ToString();
                                string? fullName = reader["ФИО"]?.ToString();

                                if (role != null && fullName != null)
                                {
                                    App.CurrentUser = new UserInfo
                                    {
                                        Role = role,
                                        FullName = fullName,
                                        Login = login
                                    };

                                    HomeWindow homeWindow = new HomeWindow();
                                    homeWindow.Show();

                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка чтения данных пользователя!", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль!", "Ошибка входа",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = new UserInfo
            {
                Role = "Гость",
                FullName = "Гость",
                Login = ""
            };

            HomeWindow homeWindow = new HomeWindow();
            homeWindow.Show();

            this.Close();
        }
    }
}