using LiveViewer.Configs;
using System;

namespace LiveViewer.ViewModel
{
    public class EntryDetailVM : BaseVM
    {
        public string TimestampTitle => $"{Constants.Labels.Timestamp}:";
        public string LevelTitle => $"{Constants.Labels.Level}:";

        private DateTime timestamp;
        public DateTime Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; NotifyPropertyChanged(); }
        }

        private string level;
        public string Level
        {
            get { return level; }
            set { level = value; NotifyPropertyChanged(); }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; NotifyPropertyChanged(); }
        }
    }
}
