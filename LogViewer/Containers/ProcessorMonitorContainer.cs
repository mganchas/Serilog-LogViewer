using System.Collections.Generic;

namespace LogViewer.Containers
{
    public static class ProcessorMonitorContainer
    {
        public static Dictionary<string, bool> ComponentStopper { get; set; } = new Dictionary<string, bool>();
    }
}
