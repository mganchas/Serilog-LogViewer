using System.Collections.Generic;
using System.Collections.ObjectModel;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.Utils
{
    public static class MessageContainer
    {
        public static Dictionary<string, ObservableCollection<LogEvent>> HttpMessages { get; set; } = new Dictionary<string, ObservableCollection<LogEvent>>();
        public static Dictionary<string, ObservableCollection<LogEvent>> FileMessages { get; set; } = new Dictionary<string, ObservableCollection<LogEvent>>();
    }
}
