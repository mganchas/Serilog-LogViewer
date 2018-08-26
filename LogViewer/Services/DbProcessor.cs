using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.ViewModel;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LogViewer.Model.Levels;

namespace LogViewer.Services
{
    public static class DbProcessor
    {
        //private static string DbName => Constants.Database.RealmName;
        //private static string DbPath => Path.Combine(Path.GetTempPath(), DbName);
        //private static RealmConfiguration RealmConfig => new RealmConfiguration(DbPath) { ShouldDeleteIfMigrationNeeded = true };

        public static void Write(string dbName, Entry entry)
        {
            using (var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(Path.GetTempPath(), dbName))))
            {
                realm.Write(() =>
                {
                    realm.Add(entry);
                });
            }
        }

        public static void Write(string dbName, Entry[] entries)
        {
            
        }

        public static List<LogEventsVM> ReadAll(string dbName)
        {
            using (var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(Path.GetTempPath(), dbName))))
            {
                return realm.All<Entry>().AsList();
            }
        }

        public static List<LogEventsVM> ReadAll(string dbName, int numberOfRows)
        {
            using (var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(Path.GetTempPath(), dbName))))
            {
                return realm.All<Entry>().AsEnumerable().Take(numberOfRows).AsList();
            }
        }

        public static List<LogEventsVM> Read(string dbName, Func<Entry, bool> predicate)
        {
            using (var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(Path.GetTempPath(), dbName))))
            {
                return realm.All<Entry>().Where(predicate).AsList();
            }
        }
        
        public static List<LogEventsVM> Read(string dbName, Func<Entry, bool> predicate, int numberOfRows)
        {
            using (var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(Path.GetTempPath(), dbName))))
            {
                return realm.All<Entry>().Where(predicate).AsEnumerable().Take(numberOfRows).AsList();
            }
        }

        public static void CleanDatabases()
        {
            var databaseFiles = Directory.GetFiles(Path.GetTempPath(), "*.*", SearchOption.AllDirectories).Where(s => Path.GetExtension(s) == "realm");
            foreach (var file in databaseFiles)
            {
                File.Delete(file);
            }
        }

        public static void CleanDatabase(string dbName)
        {
            if (!File.Exists(Path.Combine(Path.GetTempPath(), dbName))) { return; }

            var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(Path.GetTempPath(), dbName)));
            realm.Dispose();
            Realm.DeleteRealm(realm.Config);
        }
    }

    public static class DbProcessorExtensions
    {
        public static List<LogEventsVM> AsList(this IEnumerable<Entry> entries)
        {
            return entries.Select(entry => new LogEventsVM
            {
                RenderedMessage = entry.RenderedMessage,
                Timestamp = entry.Timestamp,
                LevelType = (LevelTypes)entry.LevelType
            }).ToList();
        }
    }
}
