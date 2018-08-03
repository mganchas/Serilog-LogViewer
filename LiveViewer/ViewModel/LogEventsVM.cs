using System;
using System.Windows.Media;

namespace LiveViewer.ViewModel
{
    public class LogEventsVM
    {
        public class LogEvent : BaseVM
        {
            public DateTime Timestamp { get; set; }
            public string Level { get; set; }
            public string SourceContext { get; set; }
            public string RenderedMessage { get; set; }
            public string Exception { get; set; }

            private Brush levelColor;
            public Brush LevelColor
            {
                get { return levelColor; }
                set { levelColor = value; NotifyPropertyChanged(); }
            }
        }

        public class LogEvents
        {
            public LogEvent[] Events { get; set; }
        }
    }
}
