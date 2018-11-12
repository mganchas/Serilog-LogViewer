using System;
using System.Collections.Generic;
using LogViewer.Components;
using LogViewer.Entries.Abstractions;
using LogViewer.Levels;
using LogViewer.Structures.Collections;

namespace LogViewer.Structures.Containers
{
    public static class MessageContainer
    {
        public static Dictionary<string, Lazy<ObservableSet<IEntry>>> Messages { get; } = new Dictionary<string, Lazy<ObservableSet<IEntry>>>();
        public static Dictionary<string, ObservableCounterDictionary<LevelTypes>> Counters { get; } = new Dictionary<string, ObservableCounterDictionary<LevelTypes>>();
    }
}
