using LiveViewer.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace LiveViewer.View
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

        }
    }
}
