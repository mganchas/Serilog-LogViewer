using System;
using System.Collections.Generic;
using LogViewer.Components;
using LogViewer.Components.Levels;
using LogViewer.Entries.Abstractions;
using LogViewer.Structures.Collections;

namespace LogViewer.Structures.Containers
{
    public static class MessageContainer
    {
        public static Dictionary<ComponentTypes, Dictionary<string, Lazy<ObservableSet<IEntry>>>> Messages { get; } = new Dictionary<ComponentTypes, Dictionary<string, Lazy<ObservableSet<IEntry>>>>();
        public static Dictionary<ComponentTypes, Dictionary<string, ObservableCounterDictionary<LevelTypes>>> Counters { get; } = new Dictionary<ComponentTypes, Dictionary<string, ObservableCounterDictionary<LevelTypes>>>();

        static MessageContainer()
        {
            Messages.Add(ComponentTypes.File, new Dictionary<string, Lazy<ObservableSet<IEntry>>>());
            Messages.Add(ComponentTypes.Http, new Dictionary<string, Lazy<ObservableSet<IEntry>>>());
            Messages.Add(ComponentTypes.Tcp, new Dictionary<string, Lazy<ObservableSet<IEntry>>>());
            Messages.Add(ComponentTypes.Udp, new Dictionary<string, Lazy<ObservableSet<IEntry>>>());
            
            Counters.Add(ComponentTypes.File, new Dictionary<string, ObservableCounterDictionary<LevelTypes>>());
            Counters.Add(ComponentTypes.Http, new Dictionary<string, ObservableCounterDictionary<LevelTypes>>());
            Counters.Add(ComponentTypes.Tcp, new Dictionary<string, ObservableCounterDictionary<LevelTypes>>());
            Counters.Add(ComponentTypes.Udp, new Dictionary<string, ObservableCounterDictionary<LevelTypes>>());
        }

//        public static class RAM
//        {
//            public static Dictionary<string, Lazy<ObservableSet<IEntry>>> HttpMessages { get; } = new Dictionary<string, Lazy<ObservableSet<IEntry>>>();
//            public static Dictionary<string, Lazy<ObservableSet<IEntry>>> TcpMessages { get; } = new Dictionary<string, Lazy<ObservableSet<IEntry>>>();
//            public static Dictionary<string, Lazy<ObservableSet<IEntry>>> UdpMessages { get; } = new Dictionary<string, Lazy<ObservableSet<IEntry>>>();
//            public static Dictionary<string, Lazy<ObservableSet<IEntry>>> FileMessages { get; } = new Dictionary<string, Lazy<ObservableSet<IEntry>>>();
//        }
//        public static class Disk
//        {
//            public static Dictionary<string, ObservableCounterDictionary<LevelTypes>> ComponentCounters { get; } = new Dictionary<string, ObservableCounterDictionary<LevelTypes>>();
//        }
    }
}
