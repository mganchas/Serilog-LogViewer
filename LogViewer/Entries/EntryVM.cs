using System;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using LogViewer.Components.Levels;
using LogViewer.Components.Levels.Helpers;
using LogViewer.Configs;
using LogViewer.Entries.Detached;
using LogViewer.View;
using LogViewer.ViewModelHelpers;

namespace LogViewer.Entries
{
    public class LogEventsVM : PropertyChangesNotifier
    {
        public string TimestampFormat => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        public DateTimeOffset Timestamp { get; set; }
        public string RenderedMessage { get; set; }
        public LevelTypes LevelType { get; set; }
        public Brush LevelColor => LevelTypesHelper.GetLevelColor(LevelType);

        private static string _openDialogButtonImage;
        public string OpenDialogButtonImage
        {
            get
            {
                if (string.IsNullOrEmpty(_openDialogButtonImage))
                {
                    _openDialogButtonImage = $"{Constants.Images.ImagePath}{Constants.Images.ImageSearch}";
                }
                return _openDialogButtonImage;
            }
        }

        public ICommand OpenDetailCommand => new CommandHandler(_ =>
        {
            new EntryDetailWindow(new EntryDetailVM(LevelType, RenderedMessage, Timestamp)).Show();
        });
    }
}
