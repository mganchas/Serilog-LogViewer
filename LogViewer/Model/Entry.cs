
using Newtonsoft.Json;
using Realms;
using System;
using static LogViewer.Model.Levels;

namespace LogViewer.Model
{
    public class Entry : RealmObject
    {
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("RenderedMessage")]
        public string RenderedMessage { get; set; }

        [JsonProperty("exception")]
        public string Exception { get; set; }

        public string Component { get; set; }
        public int LevelType { get; set; }
    }

    public class LogEntries
    {
        [JsonProperty("events")]
        public Entry[] Entries { get; set; }
    }
}
