using LogViewer.Model;
using LogViewer.ViewModel;

namespace LogViewer.Containers
{
    public static class LoggerContainer
    {
        public static ObservableSet<LogVM> LogEntries { get; set; } = new ObservableSet<LogVM>();
    }
}
