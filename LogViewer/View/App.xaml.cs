using LogViewer.Containers;
using LogViewer.Services;
using LogViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LoggerContainer.LogEntries.Add(new LogVM
            {
                Timestamp = DateTime.Now,
                Message = e.Exception.Message
            });
            e.Handled = true;
        }
    }
}
