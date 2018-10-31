using LogViewer.Configs;
using System;
using System.Globalization;

namespace LogViewer.ViewModel
{
    public class LogVM
    {
        public static string TypeImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageError}";

        public string VisualTimestamp => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        public DateTimeOffset Timestamp { get; set; }

        //public string Component { get; set; }

        public string Message { get; set; }
    }
}
