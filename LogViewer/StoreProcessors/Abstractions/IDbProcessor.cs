using System.Collections.Generic;
using LogViewer.Components.Levels;
using LogViewer.Entries.Abstractions;

namespace LogViewer.StoreProcessors.Abstractions
{
    public interface IDbProcessor
    {
        void WriteOne<T>(T record) where T : IEntry;
        void WriteMany<T>(IEnumerable<T> entries) where T : IEntry;
        IEnumerable<T> ReadAll<T>() where T : IEntry;
        IEnumerable<T> ReadAll<T>(int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels) where T : IEntry;
        IEnumerable<T> ReadLevels<T>(IEnumerable<LevelTypes> levels, int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text) where T : IEntry;
        IEnumerable<T> ReadLevelsAndText<T>(IEnumerable<LevelTypes> levels, string text, int numberOfRows) where T : IEntry;
        IEnumerable<T> ReadText<T>(string text) where T : IEntry;
        IEnumerable<T> ReadText<T>(string text, int numberOfRows) where T : IEntry;
        void CleanData();
    }
}