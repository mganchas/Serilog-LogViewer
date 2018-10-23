using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.ViewModel;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static LogViewer.Model.Levels;

namespace LogViewer.Services
{
    public static class DbProcessor
    {
        public static void WriteOne(string dbName, Entry entry)
        {
            using (var realm = Realm.GetInstance())
            {
                realm.Write(() =>
                {
                    realm.Add(entry);
                });
            }
        }

        public static void WriteMany(string dbName, Entry[] entries)
        {
            using (var realm = Realm.GetInstance())
            {
                realm.Write(() =>
                {
                    foreach (var entry in entries)
                    {
                        realm.Add(entry);
                    }
                });
            }
        }

        public static List<LogEventsVM> ReadAll(string dbName)
        {
            using (var realm = Realm.GetInstance())
            {
                return realm.All<Entry>().AsList();
            }
        }

        public static List<LogEventsVM> ReadAll(string dbName, int numberOfRows)
        {
            using (var realm = Realm.GetInstance())
            {
                return realm.All<Entry>().AsEnumerable().Take(numberOfRows).AsList();
            }
        }

        public static List<LogEventsVM> Read(string dbName, Func<Entry, bool> predicate)
        {
            using (var realm = Realm.GetInstance())
            {
                return realm.All<Entry>().Where(predicate).AsList();
            }
        }
        
        public static List<LogEventsVM> Read(string dbName, Func<Entry, bool> predicate, int numberOfRows)
        {
            using (var realm = Realm.GetInstance())
            {
                return realm.All<Entry>().Where(predicate).AsEnumerable().Take(numberOfRows).AsList();
            }
        }

        public static void CleanDatabases()
        {
            var databaseFiles = Directory.GetFiles(Path.GetTempPath(), "*.*", SearchOption.AllDirectories).Where(s => Path.GetExtension(s) == "realm" || Path.GetExtension(s) == "lock");
            foreach (var file in databaseFiles)
            {
                File.Delete(file);
            }
        }

        public static void CleanDatabase(string dbName)
        {
            if (!File.Exists(Path.Combine(Path.GetTempPath(), dbName))) { return; }

            var realm = Realm.GetInstance();
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
