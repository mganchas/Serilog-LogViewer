using LogViewer.Abstractions;
using LogViewer.Model;

namespace LogViewer.Containers
{
    public static class LoggerContainer
    {
        public static ObservableSet<ILog> LogEntries { get; } = new ObservableSet<ILog>();
    }
}
