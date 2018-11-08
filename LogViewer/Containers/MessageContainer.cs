using LogViewer.Model;
using System;
using System.Collections.Generic;
using static LogViewer.Model.Levels;

namespace LogViewer.Containers
{
    public static class MessageContainer
    {
        public static class RAM
        {
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> HttpMessages { get; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> TcpMessages { get; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> UdpMessages { get; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> FileMessages { get; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
        }

        public static class Disk
        {
            public static Dictionary<string, ObservableCounterDictionary<LevelTypes>> ComponentCounters { get; } = new Dictionary<string, ObservableCounterDictionary<LevelTypes>>();
        }
    }
}
