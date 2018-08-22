
using System;
using static LogViewer.Model.Levels;

namespace LogViewer.Model
{
    public class Entry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public LevelTypes LevelType { get; set; }
        public string RenderedMessage { get; set; }
        public string Component { get; set; }
    }
}
