using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using static LogViewer.Services.VisualCacheGetter;

namespace LogViewer.ViewModel
{
    public class FileComponentVM : ComponentVM, ICustomComponent
    {
        private string componentImage;
        public override string ComponentImage => GetCachedValue(ref componentImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}");

        private string executionTime;
        public string ExecutionTime
        {
            get { return $"{executionTime} miliseconds"; }
            set
            {
                executionTime = value;
                HasExecutionTime = !String.IsNullOrEmpty(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(HasExecutionTime));
            }
        }

        private bool hasExecutionTime;
        public bool HasExecutionTime
        {
            get { return hasExecutionTime; }
            set { hasExecutionTime = value; NotifyPropertyChanged(); }
        }

        private Stopwatch ExecutionWatch { get; set; }

        public FileComponentVM(string name, string path) : base(name, path, ComponentTypes.File)
        {
            // Add new message queue
            MessageContainer.RAM.FileMessages.Add(ComponentRegisterName, new Lazy<ObservableSet<Entry>>());
            
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
                    StopListener();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            };

            /* Set message collection onchanged event (RAM) */
            MessageContainer.RAM.FileMessages[ComponentRegisterName].Value.CollectionChanged += (sender, e) =>
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
                    StopListener();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            };
        }

        protected override void InitializeBackWorker()
        {
            asyncWorker.WorkerReportsProgress = true;
            asyncWorker.WorkerSupportsCancellation = true;

            // start counting execution time
            ExecutionTime = String.Empty;
            ExecutionWatch = new Stopwatch();
            ExecutionWatch.Start();

            asyncWorker.RunWorkerCompleted += delegate
            {
                if (IsRunning) { IsRunning = false; }

                // calculate execution time
                ExecutionWatch.Stop();
                ExecutionTime = ExecutionWatch.ElapsedMilliseconds.ToString();

                // play notification sound
                PlaySound();
            };

            asyncWorker.DoWork += (sender, e) =>
            {
                BackgroundWorker bwAsync = sender as BackgroundWorker;
                try
                {
                    // start file reader
                    var fp = new FileProcessor();
                    fp.ReadData(Path, ComponentRegisterName, ref asyncWorker, StoreType);

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
                    StopListener();
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

            // Check if component already exists 
            if (components.Any(x => x.Name == name || x.Path == path))
            {
                MessageBox.Show(Constants.Messages.DuplicateComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if file exists 
            if (!FileProcessor.Exists(path))
            {
                MessageBox.Show(Constants.Messages.FileNotFoundComponent, Constants.Messages.AlertTitle);
                return false;
            }

            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.RAM.FileMessages.Remove(ComponentRegisterName);
            MessageContainer.Disk.ComponentCounters.Remove(ComponentRegisterName);
        }

        public override void ClearComponent()
        {
            MessageContainer.RAM.FileMessages[ComponentRegisterName].Value.Clear();
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].Clear();
        }
    }
}
