using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace LogViewer.ViewModel
{
    public class UdpComponentVM : ComponentVM, ICustomComponent
    {
        public override string ComponentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageUdp}";
        public override string SearchImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public override string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        private string UdpFullName => Path.EndsWith("/") ? $"{Path.Substring(0, Path.Length - 1)}" : Path;
        private CancellationTokenSource cancelSource;

        public UdpComponentVM(string name, string path) : base(name, path)
        {
            // Add new message queue
            MessageContainer.UdpMessages.Add(UdpFullName, new ObservableSet<Entry>());

            /* Set message collection onchanged event */
            MessageContainer.UdpMessages[UdpFullName].CollectionChanged += (sender, e) =>
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
                    cancelSource.Cancel();
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
                    var udpP = new UdpProcessor(Path, UdpFullName);
                    udpP.ReadData(ref cancelSource, ref asyncWorker);

                    while (!e.Cancel && !cancelSource.Token.IsCancellationRequested)
                    {
                        if (bwAsync.CancellationPending)
                        {
                            e.Cancel = true;
                            cancelSource.Cancel();
                        }
                    }

                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    cancelSource.Cancel();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            };
        }

        public static bool IsValidComponent(string name, string path, in ObservableCollection<ComponentVM> components)
        {
            // Check mandatory fields 
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(name))
            {
                MessageBox.Show(Constants.Messages.MandatoryFieldsMissingComponent, Constants.Messages.AlertTitle);
                return false;
            }
            if (path.ToLower().Contains("udp"))
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
            MessageContainer.HttpMessages.Remove(UdpFullName);
        }

        public override void ClearComponent()
        {
            MessageContainer.HttpMessages[UdpFullName].Clear();
        }
    }
}