using System;
using System.Collections.Generic;
using LogViewer.Abstractions;
using LogViewer.Model;
using LogViewer.Types;

namespace LogViewer.Containers
{
    public static class MessageContainer
    {
        public static Dictionary<string, Lazy<ObservableSet<IEntry>>> Messages { get; } = new Dictionary<string, Lazy<ObservableSet<IEntry>>>();
        public static Dictionary<string, ObservableCounterDictionary<LevelTypes>> Counters { get; } = new Dictionary<string, ObservableCounterDictionary<LevelTypes>>();
    }
}
