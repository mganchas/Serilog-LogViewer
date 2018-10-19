using LogViewer.Configs;
using LogViewer.Model;
using System;
using System.Globalization;

namespace LogViewer.ViewModel
{
    public class EntryDetailVM : PropertyChangesNotifier
    {
        public string TimestampTitle => $"{Constants.Labels.Timestamp}:";
        public string LevelTitle => $"{Constants.Labels.Level}:";

        public string TimestampFormat => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        private DateTimeOffset timestamp;
        public DateTimeOffset Timestamp
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
