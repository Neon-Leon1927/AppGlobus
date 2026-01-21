using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AppGlobus.Models
{
    public class Tour : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Price { get; set; }
        public string BusType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int FreeSeats { get; set; }
        public string? PhotoFileName { get; set; } // Nullable

        // Вычисляемые свойства
        public int OccupancyPercent => Capacity > 0 ? (Capacity - FreeSeats) * 100 / Capacity : 0;

        // Процент скидки (примерная логика)
        public int DiscountPercent
        {
            get
            {
                // Пример: скидка рассчитывается от базовой цены 100000
                decimal basePrice = 100000;
                if (basePrice <= 0 || Price >= basePrice) return 0;

                decimal discount = (basePrice - Price) / basePrice * 100;
                return (int)Math.Round(discount);
            }
        }

        public bool IsSpecialOffer => DiscountPercent > 15;

        public bool IsFewSeats => Capacity > 0 && (double)FreeSeats / Capacity < 0.1;

        public bool IsStartingSoon => (StartDate - DateTime.Now).TotalDays < 7;

        // Путь к фото
        public string PhotoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(PhotoFileName))
                {
                    return $"/Images/{PhotoFileName}";
                }
                return "/Images/default_tour.png"; // Заглушка
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
