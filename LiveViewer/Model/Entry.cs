using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using static LiveViewer.Types.Levels;

namespace LiveViewer.Model
{
    [Table(Name = nameof(Entry))]
    public class Entry
    {
        [BsonElement(nameof(Id))]
        [BsonRequired()]
        public int Id { get; set; }

        [BsonElement(nameof(Timestamp))]
        [BsonRequired()]
        public DateTime Timestamp { get; set; }

        [BsonElement(nameof(LevelRaw))]
        [BsonRequired()]
        public string LevelRaw { get; set; }

        [BsonElement(nameof(LevelType))]
        [BsonRequired()]
        public LevelTypes LevelType { get; set; }

        [BsonElement(nameof(RenderedMessage))]
        [BsonRequired()]
        public string RenderedMessage { get; set; }

        [BsonElement(nameof(Component))]
        [BsonRequired()]
        public string Component { get; set; }
    }
}
