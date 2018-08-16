using GalaSoft.MvvmLight.Command;
using LiveViewer.Utils;
using System;
using System.Windows.Input;
using System.Windows.Media;
using static LiveViewer.Utils.Levels;

namespace LiveViewer.ViewModel
{
    public class LogEventsVM
    {
        public class LogEvent : BaseVM
        {
            public DateTime Timestamp { get; set; }
            public string Level { get; set; }
            public LevelTypes LevelType { get; set; }
            public string SourceContext { get; set; }
            public string RenderedMessage { get; set; }
            public string Exception { get; set; }
            public Brush LevelColor { get; set; }
            public string OpenDialogButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageSearch}";
            public ICommand OpenDetailCommand => new RelayCommand(() =>
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

        public class LogEvents
        {
            public LogEvent[] Events { get; set; }
        }
    }
}
