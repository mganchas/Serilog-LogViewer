using System;
using System.Globalization;
using LogViewer.Configs;

namespace LogViewer.Logging
{
    public class LogVM
    {
        private static string _typeImage;

        public string TypeImage
        {
            get
            {
                if (String.IsNullOrEmpty(_typeImage))
                {
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
