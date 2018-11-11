using System.Windows;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    // hook on error before app really starts
        //    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        //    base.OnStartup(e);
        //}

        //void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    // put your tracing or logging code here (I put a message box as an example)
        //    LoggerContainer.LogEntries.Add(new LogVM
        //    {
        //        Timestamp = DateTime.Now,
        //        Message = e.ExceptionObject.ToString()
        //    });
        //    //MessageBox.Show(e.ExceptionObject.ToString());
        //}

        //private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        //{
        //    LoggerContainer.LogEntries.Add(new LogVM
        //    {
        //        Timestamp = DateTime.Now,
        //        Message = e.Exception.Message
        //    });
        //    e.Handled = true;
        //}
    }
}
