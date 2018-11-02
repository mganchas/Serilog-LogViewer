using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LogViewer.Configs;
using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services;
using static LogViewer.Model.Levels;
using Expression = System.Windows.Expression;

namespace LogViewer.ViewModel
{
    public abstract class ComponentVM : PropertyChangesNotifier
    {
        public ComponentVM Self => this;
        protected BackgroundWorker asyncWorker = new BackgroundWorker();
        protected string ComponentRegisterName => $"{Name.Replace(' ', '_')}";

        private ComponentTypes Component { get; set; }

        private bool IsAllSelected { get; set; }

        private SelectionElements SelectionFilters { get; set; }

        public Dictionary<LevelTypes, LevelsVM> ComponentLevels { get; set; } = new Dictionary<LevelTypes, LevelsVM>
        {
            {LevelTypes.All, new LevelsVM(LevelTypes.All)},
            {LevelTypes.Verbose, new LevelsVM(LevelTypes.Verbose)},
            {LevelTypes.Debug, new LevelsVM(LevelTypes.Debug)},
            {LevelTypes.Information, new LevelsVM(LevelTypes.Information)},
            {LevelTypes.Warning, new LevelsVM(LevelTypes.Warning)},
            {LevelTypes.Error, new LevelsVM(LevelTypes.Error)},
            {LevelTypes.Fatal, new LevelsVM(LevelTypes.Fatal)}
        };

        #region Labels

        public static string TerminalTitle => Constants.Labels.Messages;
        public static string FilterTitle => Constants.Labels.Filters;
        public static string LevelsTitle => Constants.Labels.Levels;
        public static string EditTitle => Constants.Labels.Edit;
        public static string RemoveTitle => Constants.Labels.Remove;
        public static string StartRAMTitle => Constants.Labels.StartRAM;
        public static string StartDiskTitle => Constants.Labels.StartDisk;
        public static string StopTitle => Constants.Labels.Stop;
        public static string SaveChangesTitle => Constants.Labels.SaveChanges;

        #endregion

        #region Images

        public abstract string ComponentImage { get; }

        public static string PlayDiskImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public static string PlayRAMImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}";
        public static string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        public static string RemoveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageDelete}";
        public static string EditImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageEdit}";
        public static string SaveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageSave}";
        public static string TerminalImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageTerminal}";
        public static string MonitorImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageMonitor}";
        public static string FilterImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}";

        #endregion

        #region Visual properties

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private StoreTypes storeType;

        public StoreTypes StoreType
        {
            get { return storeType; }
            set { storeType = value; }
        }

        private string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private string filterText;

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value;
                NotifyPropertyChanged();
            }
        }

        private bool isRunning;

        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                isRunning = value;
                AllowChanges = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(AllowChanges));
            }
        }

        private bool allowChanges = true;

        public bool AllowChanges
        {
            get { return allowChanges; }
            set
            {
                allowChanges = value;
                NotifyPropertyChanged();
            }
        }

        private int visibleMessagesNr = Constants.Component.DefaultRows;

        public int VisibleMessagesNr
        {
            get { return visibleMessagesNr; }
            set
            {
                visibleMessagesNr = value;
                NotifyPropertyChanged();
            }
        }

        public List<LogEventsVM> ConsoleMessages { get; set; } = new List<LogEventsVM>();

        private HashSet<LogEventsVM> visibleConsoleMessages = new HashSet<LogEventsVM>();

        public HashSet<LogEventsVM> VisibleConsoleMessages
        {
            get { return visibleConsoleMessages; }
            set
            {
                visibleConsoleMessages = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand StartDiskListenerCommand { get; set; }
        public ICommand StartRAMListenerCommand { get; set; }
        public ICommand StopListenerCommand { get; set; }
        public ICommand CleanUpCommand { get; set; }
        public ICommand EditComponentCommand { get; set; }
        public ICommand RemoveComponentCommand { get; set; }
        public ICommand SaveChangesCommand { get; set; }
        public ICommand FilterTextSearchCommand { get; set; }
        public ICommand FilterTextClearCommand { get; set; }
        public ICommand FilterLevelCommand { get; set; }

        #endregion

        public ComponentVM(string name, string path, ComponentTypes component)
        {
            this.Name = name;
            this.Path = path;
            this.Component = component;

            // Add component to processor monitor
            ProcessorMonitorContainer.ComponentStopper.Add(this.ComponentRegisterName, false);

            // Set visible levels 
            ComponentLevels[LevelTypes.All].IsSelected = true;
            IsAllSelected = true;

            // set level counters
            MessageContainer.Disk.ComponentCounters.Add(ComponentRegisterName,
                new ObservableCounterDictionary<LevelTypes>
                {
                    {LevelTypes.All, 0},
                    {LevelTypes.Verbose, 0},
                    {LevelTypes.Debug, 0},
                    {LevelTypes.Information, 0},
                    {LevelTypes.Warning, 0},
                    {LevelTypes.Error, 0},
                    {LevelTypes.Fatal, 0}
                });

            // Set cleanup command 
            CleanUpCommand = new CommandHandler((canClean) =>
            {
                if (!(bool) canClean)
                {
                    return;
                }

                // Clear messages
                ConsoleMessages.Clear();
                VisibleConsoleMessages.Clear();
                VisibleConsoleMessages = new HashSet<LogEventsVM>();

                // clear database
                new MongoDbProcessor(ComponentRegisterName.Replace(".", "")).CleanDatabase();

                // Clear counters
                MessageContainer.Disk.ComponentCounters[ComponentRegisterName]
                    .ResetAllCounters(fireChangedEvent: false);

                // Clear visible counters 
                foreach (var lvlCounter in ComponentLevels)
                {
                    lvlCounter.Value.Counter = 0;
                }
            });

            // set start listener/reader
            StartDiskListenerCommand = new CommandHandler(_ =>
            {
                StoreType = StoreTypes.Disk;
                StartAction();
            });

            StartRAMListenerCommand = new CommandHandler(_ =>
            {
                StoreType = StoreTypes.RAM;
                StartAction();
            });

            // Set stop listener/reader command 
            StopListenerCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke((Action) (() =>
                {
                    try
                    {
                        IsRunning = false;
                        if (asyncWorker.IsBusy)
                        {
                            StopListener();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerContainer.LogEntries.Add(new LogVM
                        {
                            Timestamp = DateTime.Now,
                            Message = ex.InnerException?.Message ?? ex.Message
                        });
                        StopListener();
                    }
                }));
            });

            // Set filter search command 
            FilterTextSearchCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                    (Action) (() => { FilterMessages(); }));
            });

            // Set filter clear command 
            FilterTextClearCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
                {
                    FilterText = string.Empty;
                    FilterMessages();
                }));
            });

            // Set levels filter command
            FilterLevelCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
                {
                    if (IsAllSelected && ComponentLevels[LevelTypes.All].IsSelected &&
                        ComponentLevels.Values.Any(x => x.LevelType != LevelTypes.All && x.IsSelected))
                    {
                        ComponentLevels[LevelTypes.All].IsSelected = false;
                        IsAllSelected = false;
                    }

                    if (!IsAllSelected && ComponentLevels[LevelTypes.All].IsSelected)
                    {
                        foreach (var filterLevel in ComponentLevels)
                        {
                            if (filterLevel.Key != LevelTypes.All)
                            {
                                filterLevel.Value.IsSelected = false;
                            }
                        }

                        IsAllSelected = true;
                    }

                    FilterMessages();
                }));
            });
        }

        protected void CollectionChangedDISK(NotifyCollectionChangedEventArgs e,
            ObservableCounterDictionary<LevelTypes> originalDictionary)
        {
            if (!IsRunning || e.NewItems == null)
            {
                return;
            }

            try
            {
                // increment button counters 
                foreach (var counter in originalDictionary.GetItemSet())
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                        (Action) (() => { ComponentLevels[counter.Key].Counter = counter.Value; }));
                }

                FilterMessages();
            }
            catch (Exception ex)
            {
                LoggerContainer.LogEntries.Add(new LogVM
                {
                    Timestamp = DateTime.Now,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
                StopListener();
            }
        }

        protected void CollectionChangedRAM(NotifyCollectionChangedEventArgs e, ObservableSet<Entry> originalSet)
        {
            if (!IsRunning || e.NewItems == null)
            {
                return;
            }

            try
            {
                var entries = new HashSet<Entry>();
                foreach (Entry entry in e.NewItems)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
                    {
                        // increment specific button counter 
                        ComponentLevels[(Levels.LevelTypes) entry.LevelType].Counter++;
                        ComponentLevels[Levels.LevelTypes.All].Counter++;

                        // add item to console messages 
                        ConsoleMessages.Add(new LogEventsVM
                        {
                            RenderedMessage = entry.RenderedMessage,
                            Timestamp = entry.Timestamp,
                            LevelType = (Levels.LevelTypes) entry.LevelType
                        });
                    }));

                    entries.Add(entry);
                }

                // remove unnecessary entry from original list
                originalSet.RemoveRange(entries);

                FilterMessages();
            }
            catch (Exception ex)
            {
                LoggerContainer.LogEntries.Add(new LogVM
                {
                    Timestamp = DateTime.Now,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        private void StartAction()
        {
            try
            {
                IsRunning = true;
                ProcessorMonitorContainer.ComponentStopper[ComponentRegisterName] = false;

                if (asyncWorker.IsBusy) return;

                // Clear previous entries 
                CleanUpCommand.Execute(true);

                // Set background worker
                InitializeBackWorker();
                asyncWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                LoggerContainer.LogEntries.Add(new LogVM
                {
                    Timestamp = DateTime.Now,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
                StopListener();
            }
        }

        private void FilterMessages()
        {
            bool hasChanges = false;
            var currFilters = new SelectionElements()
            {
                FilterText = this.FilterText,
                Levels = ComponentLevels.Values.Where(x => x.IsSelected).Select(x => x.LevelType).ToList()
            };

            if (SelectionFilters == null || !SelectionFilters.Equals(currFilters))
            {
                // update current hash
                hasChanges = true;
                SelectionFilters = currFilters;
            }

            if (!hasChanges && VisibleConsoleMessages.Count == VisibleMessagesNr)
            {
                return;
            }

            // get entries from RAM or Disk
            if (StoreType == StoreTypes.Disk)
            {
                FilterEntriesDisk(currFilters.Levels);
            }
            else
            {
                FilterEntriesRAM(currFilters.Levels);
            }
        }

        private void FilterEntriesDisk(IEnumerable<LevelTypes> selectedLevels)
        {
            IEnumerable<Entry> filteredEntries = null;

            if (ComponentLevels[LevelTypes.All].IsSelected)
            {
                filteredEntries = String.IsNullOrEmpty(FilterText)
                    ? new MongoDbProcessor(ComponentRegisterName).ReadAll(VisibleMessagesNr)
                    : new MongoDbProcessor(ComponentRegisterName).ReadText(FilterText.ToLower(), VisibleMessagesNr);
            }
            else
            {
                filteredEntries = String.IsNullOrEmpty(FilterText)
                    ? new MongoDbProcessor(ComponentRegisterName).ReadLevels(selectedLevels)
                    : new MongoDbProcessor(ComponentRegisterName).ReadLevelsAndText(selectedLevels,
                        FilterText.ToLower());
            }

            var filteredList = new HashSet<LogEventsVM>();
            foreach (var entry in filteredEntries)
            {
                filteredList.Add(new LogEventsVM
                {
                    Timestamp = entry.Timestamp,
                    RenderedMessage = entry.Message + entry.RenderedMessage + entry.Exception,
                    LevelType = (LevelTypes) entry.LevelType
                });
            }

            // add filtered entries
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                (Action) (() => { VisibleConsoleMessages = new HashSet<LogEventsVM>(filteredList); }));
        }

        private void FilterEntriesRAM(IEnumerable<LevelTypes> selectedLevels)
        {
            var filteredEntries = ConsoleMessages.AsEnumerable();

            // filter level
            if (!ComponentLevels[LevelTypes.All].IsSelected)
            {
                filteredEntries = filteredEntries.Where(x => selectedLevels.Contains(x.LevelType));
            }

            // filter text
            if (!String.IsNullOrEmpty(FilterText))
            {
                var orFilters = Regex.Split(filterText, @"\||&"); // funciona o &, não funciona o | com esta expressão
                var andFilters = filterText.Split('&');

                if (orFilters.Length == 1 && andFilters.Length == 1)
                {
                    filteredEntries = filteredEntries
                        .Where(x => x.RenderedMessage.ToLower().Contains(TransformFilterString(FilterText)));
                }
                else
                {
                    // process OR's
                    foreach (var filter in orFilters)
                    {
                        
                    }

                    // process AND's
                    filteredEntries = orFilters.Aggregate(filteredEntries, (current, filter) =>
                        current.Where(x => x.RenderedMessage.ToLower().Contains(TransformFilterString(filter))));
                }
            }

            // filter visible rows
            filteredEntries = filteredEntries.Take(VisibleMessagesNr);

            // add filtered entries
            Application.Current.Dispatcher.Invoke((Action) (() =>
            {
                VisibleConsoleMessages = new HashSet<LogEventsVM>(filteredEntries);
            }));
        }

        protected void StopListener()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, (Action) (() =>
            {
                try
                {
                    asyncWorker?.CancelAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                ProcessorMonitorContainer.ComponentStopper[ComponentRegisterName] = true;
            }));
        }

        private string TransformFilterString(string filterString)
        {
            return filterString.ToLower().Trim();
        }

        protected void PlaySound()
        {
            SystemSounds.Hand.Play();
        }

        protected abstract void InitializeBackWorker();
        public abstract void RemoveComponent();
        public abstract void ClearComponent();
        public abstract bool IsValidComponent(in Span<ComponentVM> components);

        private class SelectionElements
        {
            public string FilterText { get; set; }
            public IEnumerable<LevelTypes> Levels { get; set; }

            public bool Equals(SelectionElements obj)
            {
                return this.FilterText == obj.FilterText && this.Levels.SequenceEqual(obj.Levels);
            }
        }
    }
}