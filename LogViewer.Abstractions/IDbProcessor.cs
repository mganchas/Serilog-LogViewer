using System.Collections.Generic;
using LogViewer.Types;

namespace LogViewer.Abstractions
{
    public interface IDbProcessor
    {
        void WriteOne<T>(string collection, T entry) where T : IEntry;
        void WriteMany<T>(string collection,IEnumerable<T> entries) where T : IEntry;
        IEnumerable<T> ReadAll<T>(string collection) where T : IEntry;
        IEnumerable<T> ReadAll<T>(string collection,int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadLevels<T>(string collection,IEnumerable<LevelTypes> levels) where T : IEntry;
        IEnumerable<T> ReadLevels<T>(string collection,IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadLevelsAndText<T>(string collection,IEnumerable<LevelTypes> levels, string text) where T : IEntry;
        IEnumerable<T> ReadLevelsAndText<T>(string collection,IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadText<T>(string collection,string text) where T : IEntry;
        IEnumerable<T> ReadText<T>(string collection,string text, int numberOfRows) where T : IEntry;
        void CleanData(string collection);
    }
}