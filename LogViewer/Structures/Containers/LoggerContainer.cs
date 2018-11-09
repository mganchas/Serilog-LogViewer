using LogViewer.Logging;
using LogViewer.Structures.Collections;

namespace LogViewer.Structures.Containers
{
    public static class LoggerContainer
    {
        public static ObservableSet<LogVM> LogEntries { get; } = new ObservableSet<LogVM>();
    }
}
