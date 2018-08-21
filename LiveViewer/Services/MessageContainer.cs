using LiveViewer.Model;
using LiveViewer.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LiveViewer.Services
{
    public static class MessageContainer
    {
        public static Dictionary<string, ObservableCollection<Entry>> HttpMessages { get; set; } = new Dictionary<string, ObservableCollection<Entry>>();
        public static Dictionary<string, ObservableCollection<Entry>> FileMessages { get; set; } = new Dictionary<string, ObservableCollection<Entry>>();
    }
}
