using LogViewer.Model;
using LogViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using LogViewer.Services.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using static LogViewer.Model.Levels;

namespace LogViewer.Services
{
    public class MongoDbProcessor : IDbProcessor<Entry>
    {
        private const string CollectionName = "entries";
        private readonly string _dbName;

        public MongoDbProcessor(string dbName)
        {
            this._dbName = dbName;
        }

        public void WriteOne(Entry entry)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<Entry>(CollectionName);
            collection.InsertOne(entry);
        }

        public void WriteMany(IEnumerable<Entry> entries)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<Entry>(CollectionName);
            collection.InsertMany(entries);
        }

        public IEnumerable<Entry> ReadAll()
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<Entry>(CollectionName);
            return collection.Find(_ => true).ToList();
        }

        public IEnumerable<Entry> ReadAll(int numberOfRows)
        {
            return ReadAll().Take(numberOfRows);
        }

        public IEnumerable<Entry> ReadLevels(IEnumerable<LevelTypes> levels)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<Entry>(CollectionName);
            var filter = Builders<Entry>.Filter.In(x => (LevelTypes)x.LevelType, levels);
            return collection.Find(filter).ToEnumerable();
        }

        public IEnumerable<Entry> ReadLevels(IEnumerable<LevelTypes> levels, int numberOfRows)
        {
            return ReadLevels(levels).Take(numberOfRows);
        }

        public IEnumerable<Entry> ReadLevelsAndText(IEnumerable<LevelTypes> levels, string text)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<Entry>(CollectionName);
            var builder = Builders<Entry>.Filter;
            var filter = builder.In(x => (LevelTypes)x.LevelType, levels) & builder.AnyIn(x => x.RenderedMessage.ToLower(), text);
            return collection.Find(filter).ToEnumerable();
        }

        public IEnumerable<Entry> ReadLevelsAndText(IEnumerable<LevelTypes> levels, string text, int numberOfRows)
        {
            return ReadLevelsAndText(levels, text).Take(numberOfRows);
        }

        public IEnumerable<Entry> ReadText(string text)
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<Entry>(CollectionName);
            var filter = Builders<Entry>.Filter.AnyIn(x => x.RenderedMessage.ToLower(), text);
            return collection.Find(filter).ToEnumerable();
        }

        public IEnumerable<Entry> ReadText(string text, int numberOfRows)
        {
            return ReadText(text).Take(numberOfRows);
        }

        public void CleanDatabase()
        {
            var client = new MongoClient();
            var database = client.GetDatabase(_dbName);
            database.DropCollection(CollectionName);
        }
    }
}