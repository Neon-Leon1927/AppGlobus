using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AppGlobus.Models
{
    public class Request : INotifyPropertyChanged
    {
        private string? _status;
        private string? _comment;
        private string? _tourName;

        public int Id { get; set; }
        public int TourId { get; set; }
        public int ClientId { get; set; }
        public DateTime RequestDate { get; set; }

        public string? Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public int PeopleCount { get; set; }

        // УДАЛИЛИ TotalPrice - больше не храним в модели

        public string? Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        public string? TourName
        {
            get => _tourName;
            set
            {
                if (_tourName != value)
                {
                    _tourName = value;
                    OnPropertyChanged(nameof(TourName));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
