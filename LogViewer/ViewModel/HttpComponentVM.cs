using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using Microsoft.Owin.Hosting;

namespace LogViewer.ViewModel
{
    public class HttpComponentVM : ComponentVM, ICustomComponent
    {
        public override string ComponentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}";
        private string PathFixer => !Path.EndsWith("/") ? $"{Path}/" : Path;
        private string HttpFullName => $"http://{PathFixer}";
        private CancellationTokenSource cancelSource;

        public HttpComponentVM(string name, string path) : base(name, path)
        {
            // Add new message queue
            MessageContainer.RAM.HttpMessages.Add(HttpFullName, new ObservableSet<Entry>());

            /* Set message collection onchanged event */
            MessageContainer.RAM.HttpMessages[HttpFullName].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    foreach (Entry entry in e.NewItems)
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            /* increment specific button counter */
                            ComponentLevels[(Levels.LevelTypes)entry.LevelType].Counter++;
                            ComponentLevels[Levels.LevelTypes.All].Counter++;

                            /* add item to console messages */
                            ConsoleMessages.Add(new LogEventsVM
                            {
                                RenderedMessage = entry.RenderedMessage,
                                Timestamp = entry.Timestamp,
                                LevelType = (Levels.LevelTypes)entry.LevelType
                            });
                        });
                    }

                    FilterMessages();
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
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
                    cancelSource = new CancellationTokenSource();
                    var httpP = new HttpProcessor(HttpFullName, HttpFullName);
                    httpP.ReadData(ref cancelSource, ref asyncWorker, StoreType);

                    while (!e.Cancel && !cancelSource.Token.IsCancellationRequested)
                    {
                        if (bwAsync.CancellationPending)
                        {
                            e.Cancel = true;
                            cancelSource.Cancel();
                        }
                    }

                    //using (WebApp.Start<Startup>(HttpFullName))
                    //{
                    //    while (!e.Cancel)
                    //    {
                    //        if (bwAsync.CancellationPending)
                    //        {
                    //            e.Cancel = true;
                    //        }
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            };
        }

        public static bool IsValidComponent(string name, string path, in ObservableCollection<ComponentVM> components)
        {
            // Check mandatory fields 
            if (String.IsNullOrEmpty(path)  || String.IsNullOrEmpty(name))
            {
                MessageBox.Show(Constants.Messages.MandatoryFieldsMissingComponent, Constants.Messages.AlertTitle);
                return false;
            }
            if (path.ToLower().Contains("http"))
            {
                MessageBox.Show(Constants.Messages.InvalidUrlComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if component already exists 
            if (components.Any(x => x.Name == name || x.Path == path))
            {
                MessageBox.Show(Constants.Messages.DuplicateComponent, Constants.Messages.AlertTitle);
                return false;
            }

            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.RAM.HttpMessages.Remove(HttpFullName);
        }

        public override void ClearComponent()
        {
            MessageContainer.RAM.HttpMessages[HttpFullName].Clear();
        }
    }
}
