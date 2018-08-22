using LogViewer.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace LogViewer.View
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
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
