using AppGlobus.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppGlobus
{
    /// <summary>
    /// Логика взаимодействия для BookingWindow.xaml
    /// </summary>
    public partial class BookingWindow : Window
    {
        private Tour _selectedTour;
        private int _clientId;

        public BookingWindow(Tour selectedTour)
        {
            InitializeComponent();
            _selectedTour = selectedTour;
            LoadClientInfo();
            LoadTourInfo();
        }

        private void LoadClientInfo()
        {            
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT TOP 1 [Код клиента] 
                        FROM Users 
                        WHERE Логин = @login";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", App.CurrentUser?.Login ?? "");
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            _clientId = Convert.ToInt32(result);
                        }
                        else
                        {
                            _clientId = GetOrCreateClientId(conn);
                        }
                    }
                }
            }
            catch (Exception)
            {
                _clientId = 1; 
            }
        }

        private int GetOrCreateClientId(SqlConnection conn)
        {
            string query = @"
                SELECT TOP 1 [Код пользователя] 
                FROM Users 
                WHERE Логин = @login";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@login", App.CurrentUser?.Login ?? "");
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return 1; 
                }
            }
        }

        private void LoadTourInfo()
        {
            txtTourName.Text = _selectedTour.Name;
            txtTourDates.Text = $"Дата: {_selectedTour.StartDate:dd.MM.yyyy}, " +
                              $"Длительность: {_selectedTour.Duration} дней, " +
                              $"Стоимость: {_selectedTour.Price:N0} руб./чел.";
        }

        private bool ValidateInput()
        {
            if (!int.TryParse(txtPeopleCount.Text, out int peopleCount) || peopleCount <= 0)
            {
                MessageBox.Show("Введите корректное количество человек", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPeopleCount.Focus();
                return false;
            }

            if (peopleCount > _selectedTour.FreeSeats)
            {
                MessageBox.Show($"Недостаточно свободных мест. Доступно: {_selectedTour.FreeSeats}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPeopleCount.Focus();
                return false;
            }

            return true;
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                int peopleCount = int.Parse(txtPeopleCount.Text);
                string comment = txtComment.Text.Trim();

                if (SaveBookingToDatabase(peopleCount, comment))
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при бронировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaveBookingToDatabase(int peopleCount, string comment)
        {
            try
            {
                using (var conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    string getMaxIdQuery = "SELECT ISNULL(MAX([Код заявки]), 0) + 1 FROM Aplic";
                    int newId;

                    using (var cmdId = new SqlCommand(getMaxIdQuery, conn))
                    {
                        newId = (int)cmdId.ExecuteScalar();
                    }

                    string query = @"
                INSERT INTO Aplic (
                    [Код заявки],
                    [Код тура],
                    [Код клиента],
                    [Дата заявки],
                    [Статус заявки],
                    [Количество человек],
                    [Комментарий]
                ) VALUES (
                    @id,
                    @tourId,
                    @clientId,
                    @requestDate,
                    @status,
                    @peopleCount,
                    @comment
                )";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", newId);
                        cmd.Parameters.AddWithValue("@tourId", _selectedTour.Id);
                        cmd.Parameters.AddWithValue("@clientId", _clientId);
                        cmd.Parameters.AddWithValue("@requestDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@status", "Ожидает");
                        cmd.Parameters.AddWithValue("@peopleCount", peopleCount);

                        if (string.IsNullOrEmpty(comment))
                            cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@comment", comment);

                        cmd.ExecuteNonQuery();

                        UpdateTourFreeSeats(conn, _selectedTour.Id, peopleCount);

                        MessageBox.Show($"Заявка #{newId} успешно создана!\n" +
                                      $"Забронировано мест: {peopleCount}",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);

                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void UpdateTourFreeSeats(SqlConnection conn, int tourId, int bookedSeats)
        {
            try
            {
                string checkQuery = @"
            SELECT [Свободных мест], Вместимость 
            FROM Tours 
            WHERE [Код тура] = @tourId";

                int currentFreeSeats = 0;
                int capacity = 0;

                using (var checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@tourId", tourId);
                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentFreeSeats = reader.GetInt32(0);
                            capacity = reader.GetInt32(1);
                        }
                        else
                        {
                            Console.WriteLine($"Тур с ID {tourId} не найден");
                            return;
                        }
                    }
                }

                Console.WriteLine($"Текущие свободные места: {currentFreeSeats}, Бронируем: {bookedSeats}");

                if (bookedSeats > currentFreeSeats)
                {
                    MessageBox.Show($"Недостаточно свободных мест! Доступно: {currentFreeSeats}, Запрошено: {bookedSeats}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string updateQuery = @"
            UPDATE Tours 
            SET [Свободных мест] = [Свободных мест] - @bookedSeats
            WHERE [Код тура] = @tourId";

                using (var updateCmd = new SqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@tourId", tourId);
                    updateCmd.Parameters.AddWithValue("@bookedSeats", bookedSeats);

                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Успешно обновлены свободные места для тура #{tourId}");

                        string verifyQuery = "SELECT [Свободных мест] FROM Tours WHERE [Код тура] = @tourId";
                        using (var verifyCmd = new SqlCommand(verifyQuery, conn))
                        {
                            verifyCmd.Parameters.AddWithValue("@tourId", tourId);
                            int newFreeSeats = (int)verifyCmd.ExecuteScalar();
                            Console.WriteLine($"Новое количество свободных мест: {newFreeSeats}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Не удалось обновить свободные места для тура #{tourId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления свободных мест: {ex.Message}");
            }
        }      

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
