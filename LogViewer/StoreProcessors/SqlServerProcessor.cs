using System;
using System.Collections.Generic;
using LogViewer.Components.Levels;
using LogViewer.Entries.Abstractions;
using LogViewer.StoreProcessors.Abstractions;

namespace LogViewer.StoreProcessors
{
    public sealed class SqlServerProcessor : IDbProcessor
    {
        private static readonly Lazy<SqlServerProcessor> _lazy = new Lazy<SqlServerProcessor>(() => new SqlServerProcessor());
        public static SqlServerProcessor Instance => _lazy.Value;
        
        public SqlServerProcessor()
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

        public void CleanData()
        {
            throw new System.NotImplementedException();
        }
    }
}