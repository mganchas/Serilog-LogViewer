using System.Collections.ObjectModel;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.Utils
{
    public static class MessageContainer
    {
        public static ObservableCollection<LogEvent> Messages { get; set; } = new ObservableCollection<LogEvent>();

    }
}
