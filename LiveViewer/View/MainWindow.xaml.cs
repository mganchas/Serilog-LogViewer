using LiveViewer.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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

        //protected override void OnInitialized(EventArgs e)
        //{
        //    base.OnInitialized(e);
        //    DbProcessor.CleanDatabase();
        //}

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);
        //    DbProcessor.CleanDatabase();
        //}

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] lstCaminhos = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (lstCaminhos != null)
            {
                string xpto = "";
            }
            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Released)
            {
                // Package the data.
                //DataObject data = new DataObject();
                //data.SetData("Object", this);

                // Inititate the drag-and-drop operation.
                //DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            // These Effects values are set in the drop target's
            // DragOver event handler.
            if (e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.Cross);
            }
            else if (e.Effects.HasFlag(DragDropEffects.Move))
            {
                Mouse.SetCursor(Cursors.Pen);
            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }
    }
}
