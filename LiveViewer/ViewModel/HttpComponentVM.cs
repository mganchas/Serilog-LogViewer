using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Configs;
using LiveViewer.Model;
using LiveViewer.Services;
using LiveViewer.Types;
using Microsoft.Owin.Hosting;

namespace LiveViewer.ViewModel
{
    public class HttpComponentVM : ComponentVM
    {
        public override string ComponentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}";
        public override string SearchImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public override string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        private string TransfPath => !Path.EndsWith("/") ? $"{Path}/" : Path;
        private string TransfRoute => HttpRoute.EndsWith("/") ? $"{HttpRoute.Substring(0, httpRoute.Length-1)}" : HttpRoute;
        public string HttpFullName => $"{TransfPath}{TransfRoute}";
        public static Dictionary<string, string> HttpListeners = new Dictionary<string, string>();

        private string httpRoute = Constants.Component.DefaultHttpRoute;
        public string HttpRoute
        {
            get { return httpRoute; }
            set { httpRoute = value; NotifyPropertyChanged(); }
        }

        public HttpComponentVM(string name, string path, string httpRoute) : base(name, path)
        {
            this.HttpRoute = httpRoute;

            // Add new message queue
            MessageContainer.HttpMessages.Add(HttpFullName, new ObservableCollection<Entry>());

            // Add to http listeners' list
            HttpListeners.Add(HttpFullName, ComponentRegisterName);

            // Set start/stop command 
            StartStopListenerCommand = new RelayCommand(() =>
            {
                IsRunning = !IsRunning;
                try
                {
                    if (asyncWorker.IsBusy)
                    {
                        asyncWorker.CancelAsync();
                        //timer.Stop();
                    }
                    else
                    {
                        // Set timer event
                        //timer = new Timer(Constants.Component.DefaultTimer);
                        //timer.Elapsed += delegate
                        //{
                        //    FilterMessages();
                        //};
                        //timer.Enabled = true;

                        // Set background worker 
                        InitializeBackWorker();
                        asyncWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show($"Exception occurred: {ex.Message}");
                }
            });

            /* Set message collection onchanged event */
            MessageContainer.HttpMessages[HttpFullName].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    foreach (Entry entry in e.NewItems)
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            /* increment specific button counter */
                            ComponentLevels[entry.LevelType].Counter++;
                            ComponentLevels[Levels.LevelTypes.All].Counter++;

                            /* add item to console messages */
                            ConsoleMessages.Add(new LogEventsVM
                            {
                                RenderedMessage = entry.RenderedMessage,
                                Timestamp = entry.Timestamp,
                                LevelType = entry.LevelType
                            });
                        });
                    }

                    FilterMessages();
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message);
                }
            };
        }

        protected override void InitializeBackWorker()
        {
            asyncWorker.WorkerReportsProgress = true;
            asyncWorker.WorkerSupportsCancellation = true;
            asyncWorker.RunWorkerCompleted += delegate { if (IsRunning) IsRunning = false; };
            asyncWorker.DoWork += (sender, e) =>
            {
                BackgroundWorker bwAsync = sender as BackgroundWorker;
                try
                {
                    using (WebApp.Start<Startup>(HttpFullName))
                    {
                        while (!e.Cancel)
                        {
                            if (bwAsync.CancellationPending)
                            {
                                e.Cancel = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, "Error");
                }
            };
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static bool IsValidComponent(string name, string path, string route, in ObservableCollection<ComponentVM> components)
        {
            // Check mandatory fields 
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(route) || String.IsNullOrEmpty(name))
            {
                MessageBox.Show("Component name, Http path and route are mandatory", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (!path.Contains("http"))
            {
                MessageBox.Show("Invalid Http path", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            // Check if component already exists 
            if (components.Any(x => x.Name == name || x.Path == path))
            {
                MessageBox.Show("Component already on the Components list", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.HttpMessages.Remove(HttpFullName);
        }

        public override void ClearComponent()
        {
            MessageContainer.HttpMessages[HttpFullName].Clear();
        }
    }
}
