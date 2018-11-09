using System;
using System.Collections.Generic;
using System.Linq;
using LogViewer.Components.Levels;
using LogViewer.Entries.Abstractions;
using LogViewer.Services.Abstractions;
using MongoDB.Driver;

namespace LogViewer.StoreProcessors
{
    public class MongoDBProcessor : IDbProcessor
    {
        private static readonly Lazy<MongoDBProcessor> _lazy = new Lazy<MongoDBProcessor>(() => new MongoDBProcessor());
        public static MongoDBProcessor Instance => _lazy.Value;
        
        public MongoDBProcessor()
        {
        }
        
        private const string CollectionName = "entries";
        private readonly string _dbName;

        public MongoDBProcessor(string dbName)
        {
            this._dbName = dbName;
        }

        public void WriteOne<T>(T record) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(CollectionName);
            collection.InsertOne(record);
        }

        public void WriteMany<T>(IEnumerable<T> records) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(CollectionName);
            collection.InsertMany(records);
        }

        public IEnumerable<T> ReadAll<T>() where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(CollectionName);
            return collection.Find(_ => true).ToList();
        }

        public IEnumerable<T> ReadAll<T>(int numberOfRows) where T : IEntry
        {
            return ReadAll<T>().Take(numberOfRows);
        }

        public IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(CollectionName);
            var filter = Builders<T>.Filter.In(x => (LevelTypes)x.LevelType, levels);
            return collection.Find(filter).ToEnumerable();
        }

        public IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry
        {
            return ReadLevels<T>(levels).Take(numberOfRows);
        }

        public IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(CollectionName);
            var builder = Builders<T>.Filter;
            var filter = builder.In(x => (LevelTypes)x.LevelType, levels) & builder.AnyIn(x => x.RenderedMessage.ToLower(), text);
            return collection.Find(filter).ToEnumerable();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry
        {
            return ReadLevelsAndText<T>(levels, text).Take(numberOfRows);
        }

        public IEnumerable<T> ReadText<T>(string text) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(CollectionName);
            var filter = Builders<T>.Filter.AnyIn(x => x.RenderedMessage.ToLower(), text);
            return collection.Find(filter).ToEnumerable();
        }

        public IEnumerable<T> ReadText<T>(string text, int numberOfRows) where T : IEntry
        {
            return ReadText<T>(text).Take(numberOfRows);
        }

        public void CleanDatabase()
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            database.DropCollection(CollectionName);
        }
    }
}