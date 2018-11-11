using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogViewer.ViewModelHelpers
{
    public class PropertyChangesNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void NotifyPropertyChanged([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
