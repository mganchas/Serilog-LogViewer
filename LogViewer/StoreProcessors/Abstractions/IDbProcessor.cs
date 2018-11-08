using System.Collections.Generic;
using LogViewer.Model;
using LogViewer.Model.Abstractions;

namespace LogViewer.Services.Abstractions
{
    public interface IDbProcessor
    {
        void WriteOne<T>(T record);
        void WriteMany<T>(IEnumerable<T> entries);
        IEnumerable<T> ReadAll<T>();
        IEnumerable<T> ReadAll<T>(int numberOfRows);
        IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels) where T : IEntry;
        IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text) where T : IEntry;
        IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadText<T>(string text) where T : IEntry;
        IEnumerable<T> ReadText<T>(string text, int numberOfRows) where T : IEntry;
        void CleanDatabase();
    }
}