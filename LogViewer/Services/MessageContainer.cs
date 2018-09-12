using LogViewer.Model;
using System;
using System.Collections.Generic;
using static LogViewer.Model.Levels;

namespace LogViewer.Services
{
    public static class MessageContainer
    {
        public static class RAM
        {
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> HttpMessages { get; set; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> TcpMessages { get; set; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> UdpMessages { get; set; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
            public static Dictionary<string, Lazy<ObservableSet<Entry>>> FileMessages { get; set; } = new Dictionary<string, Lazy<ObservableSet<Entry>>>();
        }

        public static class Disk
        {
            public static Dictionary<string, ObservableDictionary<LevelTypes>> ComponentCounters { get; set; } = new Dictionary<string, ObservableDictionary<LevelTypes>>();
        }
    }
}
