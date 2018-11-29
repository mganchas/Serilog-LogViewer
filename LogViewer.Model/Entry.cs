using System;
using LogViewer.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace LogViewer.Model
{
    [BsonDiscriminator("entry")]
    public class Entry : IEntry
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        [BsonElement("timestamp")]
        [JsonProperty("timestamp")] public DateTimeOffset Timestamp { get; set; }

        [BsonElement("level")]
        [JsonProperty("level")] public string Level { get; set; }

        [BsonElement("message")]
        [JsonProperty("message")] public string Message { get; set; }

        [BsonElement("renderedMessage")]
        [JsonProperty("RenderedMessage")] public string RenderedMessage { get; set; }

        [BsonElement("exception")]
        [JsonProperty("exception")] public string Exception { get; set; }

        [BsonElement("component")]
        public string Component { get; set; }

        [BsonElement("levelType")]
        public int LevelType { get; set; }
    }
}