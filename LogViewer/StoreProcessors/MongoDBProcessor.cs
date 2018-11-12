using System;
using System.Collections.Generic;
using System.Linq;
using LogViewer.Entries.Abstractions;
using LogViewer.Levels;
using LogViewer.StoreProcessors.Abstractions;
using MongoDB.Driver;

namespace LogViewer.StoreProcessors
{
    public sealed class MongoDBProcessor : IDbProcessor
    {
        private static readonly Lazy<MongoDBProcessor> _lazy = new Lazy<MongoDBProcessor>(() => new MongoDBProcessor());
        public static MongoDBProcessor Instance => _lazy.Value;
        
        public MongoDBProcessor()
        {
        }
        
        private readonly string _dbName;

        public MongoDBProcessor(string dbName)
        {
            _dbName = dbName;
        }

        public void WriteOne<T>(string collection, T entry) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var dbCollection = database.GetCollection<T>(collection);
            dbCollection.InsertOne(entry);
        }

        public void WriteMany<T>(string collection, IEnumerable<T> records) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var dbCollection = database.GetCollection<T>(collection);
            dbCollection.InsertMany(records);
        }

        public IEnumerable<T> ReadAll<T>(string collection) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var dbCollection = database.GetCollection<T>(collection);
            return dbCollection.Find(_ => true).ToList();
        }

        public IEnumerable<T> ReadAll<T>(string collection, int numberOfRows) where T : IEntry
        {
            return ReadAll<T>(collection).Take(numberOfRows);
        }

        public IEnumerable<T> ReadLevels<T>(string collection, IEnumerable<LevelTypes> levels) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var dbCollection = database.GetCollection<T>(collection);
            var filter = Builders<T>.Filter.In(x => (LevelTypes)x.LevelType, levels);
            return dbCollection.Find(filter).ToEnumerable();
        }

        public IEnumerable<T> ReadLevels<T>(string collection, IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry
        {
            return ReadLevels<T>(collection, levels).Take(numberOfRows);
        }

        public IEnumerable<T> ReadLevelsAndText<T>(string collection, IEnumerable<LevelTypes> levels, string text) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var dbCollection = database.GetCollection<T>(collection);
            var builder = Builders<T>.Filter;
            var filter = builder.In(x => (LevelTypes)x.LevelType, levels) & builder.AnyIn(x => x.RenderedMessage.ToLower(), text);
            return dbCollection.Find(filter).ToEnumerable();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(string collection, IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry
        {
            return ReadLevelsAndText<T>(collection, levels, text).Take(numberOfRows);
        }

        public IEnumerable<T> ReadText<T>(string collection, string text) where T : IEntry
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var dbCollection = database.GetCollection<T>(collection);
            var filter = Builders<T>.Filter.AnyIn(x => x.RenderedMessage.ToLower(), text);
            return dbCollection.Find(filter).ToEnumerable();
        }

        public IEnumerable<T> ReadText<T>(string collection, string text, int numberOfRows) where T : IEntry
        {
            return ReadText<T>(collection, text).Take(numberOfRows);
        }

        public void CleanData(string collection)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            database.DropCollection(collection);
        }
    }
}