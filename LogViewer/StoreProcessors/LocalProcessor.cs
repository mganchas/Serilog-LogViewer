using System;
using System.Collections.Generic;
using System.Linq;
using LogViewer.Entries.Abstractions;
using LogViewer.Levels;
using LogViewer.StoreProcessors.Abstractions;
using LogViewer.Structures.Containers;

namespace LogViewer.StoreProcessors
{
    public sealed class LocalProcessor : IDbProcessor
    {
        private static readonly Lazy<LocalProcessor> _lazy = new Lazy<LocalProcessor>(() => new LocalProcessor());
        public static LocalProcessor Instance => _lazy.Value;

        public LocalProcessor()
        {
        }

        public void WriteOne<T>(string collection, T entry) where T : IEntry
        {
            MessageContainer.Messages[collection].Value.Add(entry);
            MessageContainer.Counters[collection][LevelTypes.All]++;
            MessageContainer.Counters[collection][(LevelTypes)entry.LevelType]++;
        }

        public void WriteMany<T>(string collection, IEnumerable<T> entries) where T : IEntry
        {
            throw new System.NotImplementedException();
            MessageContainer.Messages[collection].Value.AddRange(entries as IEnumerable<IEntry>);
            MessageContainer.Counters[collection][LevelTypes.All]+= entries.Count();
            //MessageContainer.Counters[collection][(LevelTypes)entry.LevelType]++;
        }

        public IEnumerable<T> ReadAll<T>(string collection) where T : IEntry
        {
            throw new System.NotImplementedException();
            //return MessageContainer.Messages[collection].Value.GetItemSet().AsEnumerable();
        }

        public IEnumerable<T> ReadAll<T>(string collection, int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevels<T>(string collection, IEnumerable<LevelTypes> levels) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevels<T>(string collection, IEnumerable<LevelTypes> levels, int numberOfRows)
            where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(string collection, IEnumerable<LevelTypes> levels, string text)
            where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(string collection, IEnumerable<LevelTypes> levels, string text,
            int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadText<T>(string collection, string text) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadText<T>(string collection, string text, int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public void CleanData(string collection)
        {
            MessageContainer.Messages[collection].Value.Clear();
            MessageContainer.Counters[collection].ResetAllCounters();
        }
    }
}