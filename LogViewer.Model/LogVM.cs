using LogViewer.Abstractions;
using LogViewer.Resources;
using System;
using System.Globalization;

namespace LogViewer.Model
{
    public class LogVM : ILog
    {
        private static string _typeImage;
        public string TypeImage
        {
            get
            {
                if (string.IsNullOrEmpty(_typeImage)) {
                    _typeImage = $"{Constants.Images.ImagePath}{Constants.Images.ImageError}";
                }
                return _typeImage;
            }
        } 
        
        public string VisualTimestamp => Timestamp.ToString(Constants.Formats.TimeFormat, CultureInfo.InvariantCulture);
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
    }
}
