using LogViewer.Services;
using LogViewer.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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
            DbProcessor.CleanDatabase();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            DbProcessor.CleanDatabase();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            (DataContext as MainVM).AddDroppedComponents((string[])e.Data.GetData(DataFormats.FileDrop));
        }
    }
}
