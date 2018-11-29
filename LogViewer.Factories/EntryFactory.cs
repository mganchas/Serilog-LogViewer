using LogViewer.Abstractions;
using System;

namespace LogViewer.Factories
{
    public static class EntryFactory
    {
        public static IEntry CreateNewEntry(DateTimeOffset timestamp, string message, int levelType, string component, string exception)
        {
            var retObj = Activator.CreateInstance<IEntry>();
            retObj.Timestamp = timestamp;
            retObj.RenderedMessage = $"{message} {exception}";
            retObj.LevelType = levelType;
            retObj.Component = component;
            return retObj;
        }

        public static IEntry CreateNewEntry(DateTimeOffset timestamp, string message, int levelType, string component) =>
            CreateNewEntry(timestamp, message, levelType, component);
    }
}