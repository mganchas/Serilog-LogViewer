using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using LogViewer.Configs;
using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using static LogViewer.Services.VisualCacheGetter;

namespace LogViewer.ViewModel
{
    public class TcpComponentVM : ComponentVM, ICustomComponent
    {
        private string componentImage;
        public override string ComponentImage => GetCachedValue(ref componentImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageTcp}");

        private string TcpFullName => Path.EndsWith("/") ? $"{Path.Substring(0, Path.Length - 1)}" : Path;

        public TcpComponentVM(string name, string path) : base(name, path, ComponentTypes.Tcp)
        {
            /* Set message collection onchanged event (DISK) */
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        /* increment button counters */
                        foreach (var counter in (sender as ObservableCounterDictionary<Levels.LevelTypes>).GetItemSet())
                        {
                            ComponentLevels[counter.Key].Counter = counter.Value;
                        }
                    });

                    FilterMessages();
                }
                catch (Exception ex)
                {
                    LoggerContainer.LogEntries.Add(new LogVM
                    {
                        Timestamp = DateTime.Now,
                        Message = ex.Message
                    });
                    StopListener();
                }
            };

            // Add new message queue
            MessageContainer.RAM.TcpMessages.Add(ComponentRegisterName, new Lazy<ObservableSet<Entry>>());

            /* Set message collection onchanged event */
            MessageContainer.RAM.TcpMessages[ComponentRegisterName].Value.CollectionChanged += (sender, e) =>
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
                    LoggerContainer.LogEntries.Add(new LogVM
                    {
                        Timestamp = DateTime.Now,
                        Message = ex.Message
                    });
                    StopListener();
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
                    if (bwAsync.CancellationPending) { return; }

                    var tcpP = new TcpProcessor();
                    tcpP.ReadData(Path, ComponentRegisterName, ref asyncWorker, StoreType);

                    while (!e.Cancel)
                    {
                        if (bwAsync.CancellationPending)
                        {
                            e.Cancel = true;
                            StopListener();
                        }
                    }

                }
                catch (Exception ex)
                {
                    LoggerContainer.LogEntries.Add(new LogVM
                    {
                        Timestamp = DateTime.Now,
                        Message = ex.Message
                    });
                    StopListener();
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
            if (path.ToLower().Contains("tcp"))
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
            MessageContainer.RAM.TcpMessages.Remove(ComponentRegisterName);
            MessageContainer.Disk.ComponentCounters.Remove(ComponentRegisterName);
            ProcessorMonitorContainer.ComponentStopper.Remove(ComponentRegisterName);
        }

        public override void ClearComponent()
        {
            MessageContainer.RAM.TcpMessages[ComponentRegisterName].Value.Clear();
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].Clear();
        }
    }
}
