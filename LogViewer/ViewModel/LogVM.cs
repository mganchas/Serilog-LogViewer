using LogViewer.Configs;
using System;
using System.Globalization;
using static LogViewer.Services.VisualCacheGetter;

namespace LogViewer.ViewModel
{
    public class LogVM
    {
        private string typeImage;
        public string TypeImage => GetCachedValue(ref typeImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageError}");

        public string VisualTimestamp => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        public DateTimeOffset Timestamp { get; set; }

        //public string Component { get; set; }

        public string Message { get; set; }
    }
}
