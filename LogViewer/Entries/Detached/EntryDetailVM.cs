using System;
using System.Globalization;
using LogViewer.Components.Levels;
using LogViewer.Configs;
using LogViewer.ViewModelHelpers;

namespace LogViewer.Entries.Detached
{
    public class EntryDetailVM : PropertyChangesNotifier
    {
        private string _timestampTitle;
        public string TimestampTitle
        {
            get
            {
                if (string.IsNullOrEmpty(_timestampTitle)) {
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
                if (string.IsNullOrEmpty(_levelTitle)) {
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

        public EntryDetailVM(LevelTypes level, string message, DateTimeOffset timestamp)
        {
            this.Level = level.ToString();
            this.Message = message;
            this.Timestamp = timestamp;
        }
    }
}
