using AppGlobus.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace AppGlobus
{
    /// <summary>
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private ObservableCollection<Tour> _tours = new ObservableCollection<Tour>();
        public ManagerWindow()
        {
            InitializeComponent();
            Loaded += ManagerWindow_Loaded;
        }
        private void ManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTours();
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
                        ORDER BY [Код тура]";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
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
                            catch (Exception ex)
                            {
                                // Логируем ошибку чтения конкретной строки, но продолжаем
                                Console.WriteLine($"Ошибка чтения строки: {ex.Message}");
                            }
                        }
                    }
                }

                dgTours.ItemsSource = _tours;
                txtStatus.Text = $"Загружено {_tours.Count} туров";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DgTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgTours.SelectedItem != null;
            btnEditTour.IsEnabled = hasSelection;
            btnDeleteTour.IsEnabled = hasSelection;
        }

        private void BtnAddTour_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadTours();
            }
        }

        private void BtnEditTour_Click(object sender, RoutedEventArgs e)
        {
            if (dgTours.SelectedItem is Tour selectedTour)
            {
                var editWindow = new EditWindow(selectedTour);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTours();
                }
            }
        }

        private void BtnDeleteTour_Click(object sender, RoutedEventArgs e)
        {
            if (dgTours.SelectedItem is Tour selectedTour)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить тур?\n\"{selectedTour.Name}\"",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteTour(selectedTour.Id);
                }
            }
        }

        private void DeleteTour(int tourId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Tours WHERE [Код тура] = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", tourId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Тур успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadTours();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления тура: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTours();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var homeWindow = new HomeWindow();
            homeWindow.Show();
            this.Close();
        }
    }
}
