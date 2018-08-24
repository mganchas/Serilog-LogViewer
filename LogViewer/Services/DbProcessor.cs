using LogViewer.Configs;
using LogViewer.Model;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogViewer.Services
{
    public static class DbProcessor
    {
        private static string DbName => Constants.Database.RealmName;
        private static string DbPath => Path.Combine(Path.GetTempPath(), DbName);

        private static RealmConfiguration RealmConfig => new RealmConfiguration(DbPath) { ShouldDeleteIfMigrationNeeded = true };

        public static void Write(Entry entry)
        {
            using (var realm = Realm.GetInstance(RealmConfig))
            {
                realm.Write(() =>
                {
                    realm.Add(entry);
                });
            }
        }

        public static List<Entry> ReadAll()
        {
            using (var realm = Realm.GetInstance(RealmConfig))
            {
                return realm.All<Entry>().ToList();
            }
        }

        public static List<Entry> ReadAll(int numberOfRows)
        {
            using (var realm = Realm.GetInstance(RealmConfig))
            {
                return realm.All<Entry>().AsEnumerable().Take(numberOfRows).ToList();
            }
        }

        public static List<Entry> Read(Func<Entry, bool> predicate)
        {
            using (var realm = Realm.GetInstance(RealmConfig))
            {
                return realm.All<Entry>().Where(predicate).ToList();
            }
        }
        
        public static List<Entry> Read(Func<Entry, bool> predicate, int numberOfRows)
        {
            using (var realm = Realm.GetInstance(RealmConfig))
            {
                return realm.All<Entry>().Where(predicate).AsEnumerable().Take(numberOfRows).ToList();
            }
        }

        public static void CleanDatabase()
        {
            if (!File.Exists(RealmConfig.DatabasePath)) { return; }

            var realm = Realm.GetInstance(RealmConfig);
            realm.Dispose();
            Realm.DeleteRealm(realm.Config);
        }
    }
}
