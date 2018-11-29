using System.Collections.Generic;

namespace LogViewer.Containers
{
    public static class ProcessorMonitorContainer
    {
        public static Dictionary<string, bool> ComponentStopper { get; } = new Dictionary<string, bool>();
    }
}
