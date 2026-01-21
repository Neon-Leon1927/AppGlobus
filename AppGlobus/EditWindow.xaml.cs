using AppGlobus.Models;
using System;
using System.Windows;
using Microsoft.Data.SqlClient;


namespace AppGlobus
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private Tour _tour;
        private bool _isNew;

        public EditWindow(Tour tour = null)
        {
            InitializeComponent();

            if (tour == null)
            {
                _tour = new Tour();
                _isNew = true;
                Title = "Добавление нового тура";
                borderId.Visibility = Visibility.Collapsed; 
            }
            else
            {
                _tour = tour;
                _isNew = false;
                Title = "Редактирование тура";
                borderId.Visibility = Visibility.Visible;
            }

            LoadTourData();
        }

        private void LoadTourData()
        {
            if (!_isNew)
            {
                txtId.Text = _tour.Id.ToString();
            }

            txtName.Text = _tour.Name;
            txtCountry.Text = _tour.Country;
            txtDuration.Text = _tour.Duration.ToString();
            dpStartDate.SelectedDate = _tour.StartDate > DateTime.MinValue ? _tour.StartDate : DateTime.Now;
            txtPrice.Text = _tour.Price.ToString("F2");
            txtBusType.Text = _tour.BusType;
            txtCapacity.Text = _tour.Capacity.ToString();
            txtFreeSeats.Text = _tour.FreeSeats.ToString();
            txtPhoto.Text = _tour.PhotoFileName ?? "";
        }
        private bool SaveTourToDatabase()
        {
            try
            {
                using (var conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    if (_isNew)
                    {
                        string getMaxIdQuery = "SELECT ISNULL(MAX([Код тура]), 0) + 1 FROM Tours";
                        int newId;

                        using (var cmdId = new SqlCommand(getMaxIdQuery, conn))
                        {
                            newId = (int)cmdId.ExecuteScalar();
                        }

                        string query = @"
                    INSERT INTO Tours (
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
                    ) VALUES (
                        @id,
                        @name, 
                        @country, 
                        @duration, 
                        @startDate, 
                        @price, 
                        @busType, 
                        @capacity, 
                        @freeSeats, 
                        @photo
                    )";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", newId);
                            AddParameters(cmd);
                            cmd.ExecuteNonQuery();

                            _tour.Id = newId; 
                        }

                        MessageBox.Show($"Тур успешно добавлен (ID: {newId})", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string query = @"
                    UPDATE Tours SET
                        [Наименование тура] = @name,
                        Страна = @country,
                        [Продолжительность (дней)] = @duration,
                        [Дата начала] = @startDate,
                        [Стоимость (руб.)] = @price,
                        [Тип автобуса] = @busType,
                        Вместимость = @capacity,
                        [Свободных мест] = @freeSeats,
                        [Имя файла фото] = @photo
                    WHERE [Код тура] = @id";

                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", _tour.Id);
                            AddParameters(cmd);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                MessageBox.Show("Тур не найден в базе данных", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }

                        MessageBox.Show("Тур успешно обновлен", "Успех",
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                _tour.Name = txtName.Text.Trim();
                _tour.Country = txtCountry.Text.Trim();
                _tour.Duration = int.Parse(txtDuration.Text);
                _tour.StartDate = dpStartDate.SelectedDate.Value;
                _tour.Price = decimal.Parse(txtPrice.Text);
                _tour.BusType = txtBusType.Text.Trim();
                _tour.Capacity = int.Parse(txtCapacity.Text);
                _tour.FreeSeats = int.Parse(txtFreeSeats.Text);
                _tour.PhotoFileName = string.IsNullOrWhiteSpace(txtPhoto.Text) ? null : txtPhoto.Text.Trim();

                if (SaveTourToDatabase())
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



        private void AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@name", _tour.Name);
            cmd.Parameters.AddWithValue("@country", _tour.Country);
            cmd.Parameters.AddWithValue("@duration", _tour.Duration);
            cmd.Parameters.AddWithValue("@startDate", _tour.StartDate);
            cmd.Parameters.AddWithValue("@price", _tour.Price);
            cmd.Parameters.AddWithValue("@busType", _tour.BusType);
            cmd.Parameters.AddWithValue("@capacity", _tour.Capacity);
            cmd.Parameters.AddWithValue("@freeSeats", _tour.FreeSeats);

            if (string.IsNullOrEmpty(_tour.PhotoFileName))
                cmd.Parameters.AddWithValue("@photo", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@photo", _tour.PhotoFileName);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
