using LogViewer.Configs;
using LogViewer.Model;
using System;
using System.Globalization;

namespace LogViewer.ViewModel
{
    public class EntryDetailVM : PropertyChangesNotifier
    {
        private string _timestampTitle;
        public string TimestampTitle
        {
            get
            {
                if (String.IsNullOrEmpty(_timestampTitle)) {
                    _timestampTitle = $"{Constants.Labels.Timestamp}:";
                }
                return _timestampTitle;
            }
        }

        private string _levelTitle;
        public string LevelTitle
        {
            get
            {
                if (String.IsNullOrEmpty(_levelTitle)) {
                    _levelTitle = $"{Constants.Labels.Level}:";
                }
                return _levelTitle;
            }
        }

        public string TimestampFormat => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        private DateTimeOffset _timestamp;
        public DateTimeOffset Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; NotifyPropertyChanged(); }
        }

        private string _level;
        public string Level
        {
            get { return _level; }
            set { _level = value; NotifyPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyPropertyChanged(); }
        }
    }
}
