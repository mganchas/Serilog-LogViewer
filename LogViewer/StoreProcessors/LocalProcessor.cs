using System.Collections.Generic;
using LogViewer.Model;
using LogViewer.Model.Abstractions;
using LogViewer.Services.Abstractions;

namespace LogViewer.Services
{
    public class LocalProcessor : IDbProcessor
    {
        public void WriteOne<T>(T record)
        {
            throw new System.NotImplementedException();
        }

        public void WriteMany<T>(IEnumerable<T> entries)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadAll<T>()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> ReadAll<T>(int numberOfRows)
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