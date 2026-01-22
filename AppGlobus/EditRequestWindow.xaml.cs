using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using AppGlobus.Models;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppGlobus
{
    /// <summary>
    /// Логика взаимодействия для EditRequestWindow.xaml
    /// </summary>
    public partial class EditRequestWindow : Window
    {
        private Request _request;
        private bool _isNew;
        private bool _isManager;

        public EditRequestWindow(Request request = null, bool isManager = false)
        {
            InitializeComponent();

            _isManager = isManager;

            if (request == null)
            {
                _request = new Request();
                _isNew = true;
                Title = "Добавление новой заявки";
                borderId.Visibility = Visibility.Collapsed;
                dpRequestDate.IsEnabled = true;
                dpRequestDate.SelectedDate = DateTime.Now;

                if (_isManager)
                {
                    txtTourId.IsEnabled = false;
                    txtClientId.IsEnabled = false;
                    txtPeopleCount.IsEnabled = false;
                }
            }
            else
            {
                _request = request;
                _isNew = false;
                Title = "Редактирование заявки";
                borderId.Visibility = Visibility.Visible;

                if (_isManager)
                {
                    txtTourId.IsEnabled = false;
                    txtClientId.IsEnabled = false;
                    txtPeopleCount.IsEnabled = false;
                    dpRequestDate.IsEnabled = false;
                }
            }

            LoadRequestData();
        }

        private void LoadRequestData()
        {
            if (!_isNew)
            {
                txtId.Text = _request.Id.ToString();
            }

            txtTourId.Text = _request.TourId.ToString();
            txtClientId.Text = _request.ClientId.ToString();
            dpRequestDate.SelectedDate = _request.RequestDate > DateTime.MinValue ?
                _request.RequestDate : DateTime.Now;
            txtPeopleCount.Text = _request.PeopleCount.ToString();
            txtComment.Text = _request.Comment ?? "";

            if (!string.IsNullOrEmpty(_request.Status))
            {
                foreach (var item in cbStatus.Items)
                {
                    if (item is ComboBoxItem comboBoxItem &&
                        comboBoxItem.Content.ToString() == _request.Status)
                    {
                        cbStatus.SelectedItem = item;
                        break;
                    }
                }
            }

            if (cbStatus.SelectedItem == null && cbStatus.Items.Count > 0)
            {
                cbStatus.SelectedIndex = 0;
            }
        }


        private bool ValidateInput()
        {
            if (!int.TryParse(txtTourId.Text, out int tourId) || tourId <= 0)
            {
                MessageBox.Show("Введите корректный код тура", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTourId.Focus();
                return false;
            }

            if (!int.TryParse(txtClientId.Text, out int clientId) || clientId <= 0)
            {
                MessageBox.Show("Введите корректный код клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClientId.Focus();
                return false;
            }

            if (dpRequestDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заявки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpRequestDate.Focus();
                return false;
            }

            if (!int.TryParse(txtPeopleCount.Text, out int peopleCount) || peopleCount <= 0)
            {
                MessageBox.Show("Введите корректное количество человек", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPeopleCount.Focus();
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                _request.TourId = int.Parse(txtTourId.Text);
                _request.ClientId = int.Parse(txtClientId.Text);
                _request.RequestDate = dpRequestDate.SelectedDate.Value;
                _request.PeopleCount = int.Parse(txtPeopleCount.Text);
                _request.Comment = string.IsNullOrWhiteSpace(txtComment.Text) ?
                    null : txtComment.Text.Trim();

                if (cbStatus.SelectedItem is ComboBoxItem selectedItem)
                {
                    _request.Status = selectedItem.Content.ToString();
                }

                // Сохраняем в БД
                if (SaveRequestToDatabase())
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool SaveRequestToDatabase()
        {
            try
            {
                using (var conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    if (_isNew)
                    {
                        // Получаем максимальный ID
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
                            cmd.Parameters.AddWithValue("@tourId", _request.TourId);
                            cmd.Parameters.AddWithValue("@clientId", _request.ClientId);
                            cmd.Parameters.AddWithValue("@requestDate", _request.RequestDate);
                            cmd.Parameters.AddWithValue("@status", _request.Status ?? "Ожидает");
                            cmd.Parameters.AddWithValue("@peopleCount", _request.PeopleCount);

                            if (string.IsNullOrEmpty(_request.Comment))
                                cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@comment", _request.Comment);

                            cmd.ExecuteNonQuery();

                            _request.Id = newId;
                        }

                        MessageBox.Show($"Заявка успешно добавлена (ID: {newId})", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string query = @"
                    UPDATE Aplic SET
                        [Код тура] = @tourId,
                        [Код клиента] = @clientId,
                        [Дата заявки] = @requestDate,
                        [Статус заявки] = @status,
                        [Количество человек] = @peopleCount,
                        [Комментарий] = @comment
                    WHERE [Код заявки] = @id";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", _request.Id);
                            cmd.Parameters.AddWithValue("@tourId", _request.TourId);
                            cmd.Parameters.AddWithValue("@clientId", _request.ClientId);
                            cmd.Parameters.AddWithValue("@requestDate", _request.RequestDate);
                            cmd.Parameters.AddWithValue("@status", _request.Status ?? "Ожидает");
                            cmd.Parameters.AddWithValue("@peopleCount", _request.PeopleCount);

                            if (string.IsNullOrEmpty(_request.Comment))
                                cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@comment", _request.Comment);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                MessageBox.Show("Заявка не найдена в базе данных", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }

                        MessageBox.Show("Заявка успешно обновлена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    return true;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }       

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
