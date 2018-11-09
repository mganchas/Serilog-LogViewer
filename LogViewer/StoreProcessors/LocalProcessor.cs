using System;
using System.Collections.Generic;
using LogViewer.Components.Levels;
using LogViewer.Entries.Abstractions;
using LogViewer.Services.Abstractions;

namespace LogViewer.StoreProcessors
{
    public sealed class LocalProcessor : IDbProcessor
    {
        private static readonly Lazy<LocalProcessor> _lazy = new Lazy<LocalProcessor>(() => new LocalProcessor());
        public static LocalProcessor Instance => _lazy.Value;

        public LocalProcessor()
        {
        }

        public void WriteOne<T>(T record) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public void WriteMany<T>(IEnumerable<T> entries) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadAll<T>() where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadAll<T>(int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadText<T>(string text) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadText<T>(string text, int numberOfRows) where T : IEntry
        {
            throw new System.NotImplementedException();
        }

        public void CleanDatabase()
        {
            throw new System.NotImplementedException();
        }
    }
}