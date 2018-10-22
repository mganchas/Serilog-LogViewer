using LogViewer.Model;
using System.Collections.Generic;

namespace LogViewer.Services
{
    public class ProcessorMonitor
    {
        public static Dictionary<string, bool> ComponentStopper { get; set; } = new Dictionary<string, bool>();
    }
}
