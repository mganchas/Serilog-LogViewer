using System.Windows;
using System.Windows.Input;
using LogViewer.ViewModel;

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
