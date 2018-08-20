using LiveViewer.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.IO.Ports;
using System.Linq;
using static LiveViewer.Types.Levels;

namespace LiveViewer.Services
{
    public sealed class DbProcessor
    {
        //private static string DbPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Name);
        private static string Name => "LiveViewer";
        private static string DbConnection => "mongodb://localhost";

        //static DbProcessor()
        //{
        //    if (!Directory.Exists(DbPath)) {
        //        Directory.CreateDirectory(DbPath);
        //    }
        //}

        private IMongoCollection<Entry> GetCollection()
        {
            var client = new MongoClient(DbConnection);
            var database = client.GetDatabase(Name);
            var coll = database.GetCollection<Entry>(nameof(Entry));
            return coll;
        }

        public void InsertOne(Entry entry)
        {
            var coll = GetCollection();
            ss();
            //coll.InsertOne(entry);
        }

        public void InsertMany(Entry[] entries)
        {
            var coll = GetCollection();
            coll.InsertMany(entries);
        }

        public IEnumerable<Entry> GetAllEntries(string component)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component)
                             .ToEnumerable();

            return dbData;
        }

        public IEnumerable<Entry> GetAllEntriesWithText(string component, string filterText)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component &&
                                        x.RenderedMessage.ToLower().Contains(filterText))
                             .ToEnumerable();

            return dbData;
        }

        public IEnumerable<Entry> GetNEntriesWithText(string component, string filterText, int numberOfEntries)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component &&
                                        x.RenderedMessage.ToLower().Contains(filterText))
                             .ToEnumerable()
                             .Take(numberOfEntries);

            return dbData;
        }

        public IEnumerable<Entry> GetAllEntriesWithLevels(string component, IEnumerable<LevelTypes> levels)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component &&
                                        levels.Contains(x.LevelType))
                             .ToEnumerable();

            return dbData;
        }

        public IEnumerable<Entry> GetNEntriesWithLevels(string component, IEnumerable<LevelTypes> levels, int numberOfEntries)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component &&
                                        levels.Contains(x.LevelType))
                             .ToEnumerable()
                             .Take(numberOfEntries);

            return dbData;
        }

        public IEnumerable<Entry> GetAllEntriesWithTextAndLevels(string component, string filterText, IEnumerable<LevelTypes> levels)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component &&
                                        levels.Contains(x.LevelType) &&
                                        x.RenderedMessage.ToLower().Contains(filterText))
                             .ToEnumerable();

            return dbData;
        }

        public IEnumerable<Entry> GetNEntriesWithTextAndLevels(string component, string filterText, IEnumerable<LevelTypes> levels, int numberOfEntries)
        {
            var coll = GetCollection();
            var dbData = coll.Find(x => x.Component == component &&
                                        levels.Contains(x.LevelType) &&
                                        x.RenderedMessage.ToLower().Contains(filterText))
                             .ToEnumerable()
                             .Take(numberOfEntries);

            return dbData;
        }

        public static void CleanDatabase()
        {
            //using (var ctx = new DatabaseContext())
            //{
            //    ctx.Database.Delete();
            //    ctx.SaveChanges();
            //}
        }
    }
}
