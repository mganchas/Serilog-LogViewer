using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogViewer.Model;

namespace LogViewer.Services.Abstractions
{
    public interface IDbProcessor<T>
    {
        void WriteOne(T entry);
        void WriteMany(IEnumerable<T> entries);
        IEnumerable<T> ReadAll();
        IEnumerable<T> ReadAll(int numberOfRows);
        IEnumerable<T> ReadLevels(IEnumerable<Levels.LevelTypes> levels);
        IEnumerable<T> ReadLevels(IEnumerable<Levels.LevelTypes> levels, int numberOfRows);
        IEnumerable<T> ReadLevelsAndText(IEnumerable<Levels.LevelTypes> levels, string text);
        IEnumerable<T> ReadLevelsAndText(IEnumerable<Levels.LevelTypes> levels, string text, int numberOfRows);
        IEnumerable<T> ReadText(string text);
        IEnumerable<T> ReadText(string text, int numberOfRows);
        void CleanDatabase();
    }
}