using LiveViewer.ViewModel;
using System.Windows;

namespace LiveViewer.View
{
    /// <summary>
    /// Interaction logic for EntryDetailWindow.xaml
    /// </summary>
    public partial class EntryDetailWindow : Window
    {
        public EntryDetailWindow(EntryDetailVM vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
