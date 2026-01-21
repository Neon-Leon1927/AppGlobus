using AppGlobus.Models;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AppGlobus
{
    /// <summary>
    /// Логика взаимодействия для HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private ObservableCollection<Tour> _tours = new ObservableCollection<Tour>();
        public HomeWindow()
        {
            InitializeComponent();
            Loaded += HomeWindow_Loaded;
        }
        private void HomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Показываем информацию о пользователе
            if (App.CurrentUser != null)
            {
                txtUserInfo.Text = $"{App.CurrentUser.FullName} ({App.CurrentUser.Role})";

                // Показываем кнопки управления только для менеджеров и администраторов
                if (App.CurrentUser.Role == "Менеджер" || App.CurrentUser.Role == "Администратор")
                {
                    btnManageTours.Visibility = Visibility.Visible;
                    btnRequests.Visibility = Visibility.Visible;
                }
            }
            else
            {
                txtUserInfo.Text = "Гость";
            }

            LoadTours();
        }
        private void BtnManageTours_Click(object sender, RoutedEventArgs e)
        {
            var toursWindow = new ManagerWindow();
            toursWindow.Show();
            this.Close();
        }
        private void LoadTours()
        {
            try
            {
                _tours.Clear();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    [Код тура],
                    [Наименование тура],
                    Страна,
                    [Продолжительность (дней)],
                    [Дата начала],
                    [Стоимость (руб.)],
                    [Тип автобуса],
                    Вместимость,
                    [Свободных мест],
                    [Имя файла фото]
                FROM Tours
                ORDER BY [Дата начала]";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tour = new Tour
                            {
                                Id = reader.GetInt("Код тура"),
                                Name = reader.GetString("Наименование тура"),
                                Country = reader.GetString("Страна"),
                                Duration = reader.GetInt("Продолжительность (дней)"),
                                StartDate = reader.GetDateTime("Дата начала"),
                                Price = reader.GetDecimal("Стоимость (руб.)"),
                                BusType = reader.GetString("Тип автобуса"),
                                Capacity = reader.GetInt("Вместимость"),
                                FreeSeats = reader.GetInt("Свободных мест"),
                                PhotoFileName = reader.GetStringOrNull("Имя файла фото")
                            };

                            _tours.Add(tour);
                        }
                    }
                }

                itemsTours.ItemsSource = _tours;
                txtStatus.Text = "Туры загружены";
                txtToursCount.Text = _tours.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnRequests_Click(object sender, RoutedEventArgs e)
        {
            // Вместо ManagerWindow открываем RequestsWindow
            var requestsWindow = new RequestsWindow();
            requestsWindow.Show();
            this.Close();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnBookTour_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int tourId))
            {
                var selectedTour = _tours.FirstOrDefault(t => t.Id == tourId);

                if (selectedTour != null)
                {
                    if (App.CurrentUser?.Role == "Авторизированный клиент")
                    {
                        var bookingWindow = new BookingWindow(selectedTour);
                        if (bookingWindow.ShowDialog() == true)
                        {
                            // Обновляем список туров (количество свободных мест изменилось)
                            LoadTours();

                            MessageBox.Show($"Заявка на тур '{selectedTour.Name}' оформлена!\n" +
                                          $"С вами свяжется менеджер для подтверждения.",
                                          "Заявка оформлена",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Только авторизованные клиенты могут оформлять заявки",
                                      "Внимание",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
