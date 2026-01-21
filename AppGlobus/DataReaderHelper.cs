using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace AppGlobus
{
    public static class DataReaderHelper
    {
        public static T GetValue<T>(this SqlDataReader reader, string columnName, T defaultValue = default)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return defaultValue;

                object value = reader[ordinal];
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int GetInt(this SqlDataReader reader, string columnName, int defaultValue = 0)
        {
            return GetValue(reader, columnName, defaultValue);
        }

        public static decimal GetDecimal(this SqlDataReader reader, string columnName, decimal defaultValue = 0)
        {
            return GetValue(reader, columnName, defaultValue);
        }

        public static string GetString(this SqlDataReader reader, string columnName, string defaultValue = "")
        {
            return GetValue(reader, columnName, defaultValue);
        }

        public static string? GetStringOrNull(this SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader[ordinal].ToString();
            }
            catch
            {
                return null;
            }
        }

        public static DateTime GetDateTime(this SqlDataReader reader, string columnName, DateTime defaultValue = default)
        {
            if (defaultValue == default)
                defaultValue = DateTime.MinValue;

            return GetValue(reader, columnName, defaultValue);
        }
    }
}
