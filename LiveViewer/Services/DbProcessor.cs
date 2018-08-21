using LiveViewer.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using static LiveViewer.Types.Levels;

namespace LiveViewer.Services
{
    public sealed class DbProcessor
    {
        public void InsertOne(Entry entry)
        {
            using (var db = new DatabaseContext())
            {
                db.Entries.Add(entry);
                db.SaveChanges();
            }
        }

        public void InsertMany(Entry[] entries)
        {
            using (var db = new DatabaseContext())
            {
                db.Entries.AddRange(entries);
                db.SaveChanges();
            }
        }

        public Task<List<Entry>> GetAllEntries(string component, int numberOfEntries)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component)).Take(numberOfEntries).ToListAsync();
            }
        }

        public Task<List<Entry>> GetAllEntries(string component)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component)).ToListAsync();
            }
        }
        
        public Task<List<Entry>> GetEntriesWithText(string component, string filterText, int numberOfEntries)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component && x.RenderedMessage.ToLower().Contains(filterText)).Take(numberOfEntries)).ToListAsync();
            }
        }

        public Task<List<Entry>> GetEntriesWithText(string component, string filterText)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component && x.RenderedMessage.ToLower().Contains(filterText))).ToListAsync();
            }
        }

        public Task<List<Entry>> GetEntriesWithLevels(string component, IEnumerable<LevelTypes> levels, int numberOfEntries)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component && levels.Contains(x.LevelType))).Take(numberOfEntries).ToListAsync();
            }
        }

        public Task<List<Entry>> GetEntriesWithLevels(string component, IEnumerable<LevelTypes> levels)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component && levels.Contains(x.LevelType))).ToListAsync();
            }
        }

        public Task<List<Entry>> GetEntriesWithTextAndLevels(string component, string filterText, IEnumerable<LevelTypes> levels, int numberOfEntries)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component && levels.Contains(x.LevelType) && x.RenderedMessage.ToLower().Contains(filterText))).Take(numberOfEntries).ToListAsync();
            }
        }

        public Task<List<Entry>> GetEntriesWithTextAndLevels(string component, string filterText, IEnumerable<LevelTypes> levels)
        {
            using (var db = new DatabaseContext())
            {
                return new EntryDbAsyncEnumerable<Entry>(db.Entries.Where(x => x.Component == component && levels.Contains(x.LevelType) && x.RenderedMessage.ToLower().Contains(filterText))).ToListAsync();
            }
        }
        
        public static void CleanDatabase()
        {
            DatabaseContext.KillDatabase();
        }
    }
}
