using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace AppGlobus
{
    public static class DatabaseHelper
    {
        // Исправляем: одна простая строка подключения
        public static string ConnectionString =>
            @"Server=(localdb)\MSSQLLocalDB;Database=Globusbd;Integrated Security=True;";

        public static bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка БД");
                return false;
            }
        }


    }
}
