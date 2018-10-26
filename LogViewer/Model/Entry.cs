using Newtonsoft.Json;
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogViewer.Model
{
    [BsonDiscriminator("entry")]
    public class Entry //: RealmObject
    {
        [BsonElement("timestamp")]
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [BsonElement("level")]
        [JsonProperty("level")]
        public string Level { get; set; }

        [BsonElement("message")]
        [JsonProperty("message")]
        public string Message { get; set; }

        [BsonElement("renderedMessage")]
        [JsonProperty("RenderedMessage")]
        public string RenderedMessage { get; set; }

        [BsonElement("exception")]
        [JsonProperty("exception")]
        public string Exception { get; set; }

        [BsonIgnore]
        public string Component { get; set; }

        [BsonIgnore]
        public int LevelType { get; set; }
    }

    public class LogEntries
    {
        [JsonProperty("events")]
        public Entry[] Entries { get; set; }
    }
}
