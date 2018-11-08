using LogViewer.Model;
using LogViewer.ViewModel;

namespace LogViewer.Containers
{
    public static class LoggerContainer
    {
        public static ObservableSet<LogVM> LogEntries { get; } = new ObservableSet<LogVM>();
    }
}
