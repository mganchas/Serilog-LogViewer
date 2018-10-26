using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.View;
using System;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using static LogViewer.Model.Levels;

namespace LogViewer.ViewModel
{
    public class LogEventsVM : PropertyChangesNotifier
    {
        public string TimestampFormat => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        public DateTimeOffset Timestamp { get; set; }
        public string RenderedMessage { get; set; }
        public LevelTypes LevelType { get; set; }
        public Brush LevelColor => Levels.GetLevelColor(LevelType);

        public string OpenDialogButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageSearch}";

        public ICommand OpenDetailCommand => new CommandHandler(_ =>
        {
            var wind = new EntryDetailWindow(new EntryDetailVM
            {
                Level = this.LevelType.ToString(),
                Message = this.RenderedMessage,
                Timestamp = this.Timestamp
            });
            wind.Show();
        });
    }
}
