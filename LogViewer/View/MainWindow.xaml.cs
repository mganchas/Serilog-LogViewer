using LogViewer.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;

namespace LogViewer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //DbProcessor.CleanDatabases();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            //DbProcessor.CleanDatabases();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            (DataContext as MainVM).AddDroppedComponents((string[])e.Data.GetData(DataFormats.FileDrop));
        }
    }
}
