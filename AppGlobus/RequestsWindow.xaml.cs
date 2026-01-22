using System;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AppGlobus.Models;

namespace AppGlobus
{
    /// <summary>
    /// Логика взаимодействия для RequestsWindow.xaml
    /// </summary>
    public partial class RequestsWindow : Window
    {
        private ObservableCollection<Request> _requests = new ObservableCollection<Request>();
        private bool _isManager = false;

        public RequestsWindow()
        {
            InitializeComponent();
            Loaded += RequestsWindow_Loaded;
        }

        private void RequestsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Определяем роль пользователя
            if (App.CurrentUser?.Role == "Менеджер")
            {
                _isManager = true;
                btnAddRequest.Visibility = Visibility.Collapsed;
                btnDeleteRequest.Visibility = Visibility.Collapsed;
                Title = "Просмотр заявок (Менеджер)";
            }
            else if (App.CurrentUser?.Role == "Администратор")
            {
                _isManager = false;
                Title = "Управление заявками (Администратор)";
            }

            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                _requests.Clear();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Упрощенный запрос без стоимости
                    string query = @"
                SELECT 
                    a.[Код заявки],
                    a.[Код тура],
                    a.[Код клиента],
                    a.[Дата заявки],
                    a.[Статус заявки],
                    a.[Количество человек],
                    a.[Комментарий],
                    t.[Наименование тура]
                FROM Aplic a
                LEFT JOIN Tours t ON a.[Код тура] = t.[Код тура]
                ORDER BY a.[Дата заявки] DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var request = new Request
                            {
                                Id = GetIntSafe(reader, "Код заявки"),
                                TourId = GetIntSafe(reader, "Код тура"),
                                ClientId = GetIntSafe(reader, "Код клиента"),
                                RequestDate = GetDateTimeSafe(reader, "Дата заявки"),
                                Status = GetStringSafe(reader, "Статус заявки"),
                                PeopleCount = GetIntSafe(reader, "Количество человек"),
                                Comment = GetStringOrNullSafe(reader, "Комментарий"),
                                TourName = GetStringSafe(reader, "Наименование тура")
                            };

                            _requests.Add(request);
                        }
                    }
                }

                dgRequests.ItemsSource = _requests;
                txtStatus.Text = $"Загружено {_requests.Count} заявок";
                txtRequestsCount.Text = $"Заявок: {_requests.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Вспомогательные методы для безопасного чтения
        private int GetIntSafe(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
            }
            catch
            {
                return 0;
            }
        }

        private string GetStringSafe(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
            }
            catch
            {
                return string.Empty;
            }
        }

        private DateTime GetDateTimeSafe(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private decimal GetDecimalSafe(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetDecimal(ordinal);
            }
            catch
            {
                return 0;
            }
        }

        private string? GetStringOrNullSafe(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private void DgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgRequests.SelectedItem != null;
            btnEditRequest.IsEnabled = hasSelection;

            // Администратор может удалять, менеджер - нет
            if (!_isManager)
            {
                btnDeleteRequest.IsEnabled = hasSelection;
            }
        }

        private void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            // Окно для добавления новой заявки
            var editWindow = new EditRequestWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadRequests();
            }
        }
        private void BtnCheckData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT TOP 5
                    a.[Код заявки],
                    a.[Количество человек],
                    a.[Общая стоимость (руб.)],
                    t.[Стоимость (руб.)] as ЦенаЗаЧеловека,
                    a.[Количество человек] * t.[Стоимость (руб.)] as ДолжноБыть
                FROM Aplic a
                LEFT JOIN Tours t ON a.[Код тура] = t.[Код тура]
                ORDER BY a.[Код заявки] DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string info = "Проверка данных (последние 5 заявок):\n\n";

                        while (reader.Read())
                        {
                            int requestId = reader.GetInt32(0);
                            int peopleCount = reader.GetInt32(1);
                            decimal currentPrice = reader.GetDecimal(2);
                            decimal pricePerPerson = reader.GetDecimal(3);
                            decimal shouldBe = reader.GetDecimal(4);

                            info += $"Заявка #{requestId}:\n";
                            info += $"  Людей: {peopleCount}\n";
                            info += $"  Цена за чел: {pricePerPerson:N0} руб.\n";
                            info += $"  Текущая стоимость: {currentPrice:N0} руб.\n";
                            info += $"  Должно быть: {shouldBe:N0} руб.\n";
                            info += $"  Совпадение: {(currentPrice == shouldBe ? "✅" : "❌")}\n\n";
                        }

                        MessageBox.Show(info, "Проверка данных",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnEditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem is Request selectedRequest)
            {
                // Окно редактирования заявки
                var editWindow = new EditRequestWindow(selectedRequest, _isManager);
                if (editWindow.ShowDialog() == true)
                {
                    LoadRequests();
                }
            }
        }

        private void BtnDeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem is Request selectedRequest)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить заявку #{selectedRequest.Id}?\n",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteRequest(selectedRequest.Id);
                }
            }
        }

        private void DeleteRequest(int requestId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Aplic WHERE [Код заявки] = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", requestId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Заявка успешно удалена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadRequests();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var homeWindow = new HomeWindow();
            homeWindow.Show();
            this.Close();
        }
    }
}
