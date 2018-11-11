using LogViewer.ViewModelHelpers;

namespace LogViewer.Components.Detached
{
    public class VisibleMessagesVM : PropertyChangesNotifier
    {
        private string windowTitle;
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                NotifyPropertyChanged();
            }
        }
        
        public ComponentVM Component { get; set; }
        
        public VisibleMessagesVM(ComponentVM component)
        {
            this.Component = component;
            this.Component.CanExport = false;
            this.WindowTitle = $"{component.Name}: {component.Path}";
        }
    }
}