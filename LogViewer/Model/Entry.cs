
using Newtonsoft.Json;
using System;
using static LogViewer.Model.Levels;

namespace LogViewer.Model
{
    public class Entry
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("message")]
        public string RenderedMessage { get; set; }

        [JsonProperty("exception")]
        public string Exception { get; set; }

        public string Component { get; set; }
        public LevelTypes LevelType { get; set; }
    }

    public class LogEntries
    {
        public Entry[] Entries { get; set; }
    }
}
