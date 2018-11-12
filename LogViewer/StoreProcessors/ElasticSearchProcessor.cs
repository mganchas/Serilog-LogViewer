using System;
using System.Collections.Generic;
using LogViewer.Entries.Abstractions;
using LogViewer.Levels;
using LogViewer.StoreProcessors.Abstractions;

namespace LogViewer.StoreProcessors
{
    public sealed class ElasticSearchProcessor : IDbProcessor
    {
        private static readonly Lazy<ElasticSearchProcessor> _lazy = new Lazy<ElasticSearchProcessor>(() => new ElasticSearchProcessor());
        public static ElasticSearchProcessor Instance => _lazy.Value;
        
        public ElasticSearchProcessor()
        {
        }

        public void WriteOne<T>(string collection, T entry) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public void WriteMany<T>(string collection, IEnumerable<T> entries) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadAll<T>(string collection) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadAll<T>(string collection, int numberOfRows) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadLevels<T>(string collection, IEnumerable<LevelTypes> levels) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadLevels<T>(string collection, IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(string collection, IEnumerable<LevelTypes> levels, string text) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(string collection, IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadText<T>(string collection, string text) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadText<T>(string collection, string text, int numberOfRows) where T : IEntry
        {
            throw new NotImplementedException();
        }

        public void CleanData(string collection)
        {
            throw new NotImplementedException();
        }
    }
}