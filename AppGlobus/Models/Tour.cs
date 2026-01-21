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
        public string? PhotoFileName { get; set; } 
                
        public int OccupancyPercent => Capacity > 0 ? (Capacity - FreeSeats) * 100 / Capacity : 0;

        public int DiscountPercent
        {
            get
            {
                decimal basePrice = 100000;
                if (basePrice <= 0 || Price >= basePrice) return 0;

                decimal discount = (basePrice - Price) / basePrice * 100;
                return (int)Math.Round(discount);
            }
        }

        public bool IsSpecialOffer => DiscountPercent > 15;

        public bool IsFewSeats => Capacity > 0 && (double)FreeSeats / Capacity < 0.1;

        public bool IsStartingSoon => (StartDate - DateTime.Now).TotalDays < 7;

        public string PhotoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(PhotoFileName))
                {
                    return $"/Images/{PhotoFileName}";
                }
                return "/Images/default_tour.png"; 
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
