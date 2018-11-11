using System.Windows;
using LogViewer.Components;
using LogViewer.Components.Detached;

namespace LogViewer.View
{
    public partial class VisibleMessagesWindow : Window
    {
        public VisibleMessagesWindow(ComponentVM vm)
        {
            this.DataContext = new VisibleMessagesVM(vm);
            InitializeComponent();
        }
    }
}
