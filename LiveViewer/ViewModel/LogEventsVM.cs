using GalaSoft.MvvmLight.Command;
using LiveViewer.Configs;
using LiveViewer.Model;
using LiveViewer.Types;
using LiveViewer.View;
using System;
using System.Windows.Input;
using System.Windows.Media;
using static LiveViewer.Types.Levels;

namespace LiveViewer.ViewModel
{
    public class LogEventsVM : BaseVM
    {
        public DateTime Timestamp { get; set; }
        public string RenderedMessage { get; set; }
        public string LevelRaw { get; set; }
        public Brush LevelColor { get; set; }

        public LevelTypes LevelType => Levels.GetLevelTypeFromString(LevelRaw);
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
        public Entry[] Events { get; set; }
    }
}
