using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using static LogViewer.Model.Levels;
using static LogViewer.Services.VisualCacheGetter;

namespace LogViewer.ViewModel
{
    public abstract class ComponentVM : PropertyChangesNotifier, ICustomComponent
    {
        public ComponentVM Self => this;
        protected BackgroundWorker asyncWorker = new BackgroundWorker();
        protected string ComponentRegisterName => $"{Name.Replace(' ', '_')}";

        private ComponentTypes Component { get; set; }

        private bool IsAllSelected { get; set; }

        private SelectionElements SelectionFilters { get; set; }

        public Dictionary<LevelTypes, LevelsVM> ComponentLevels { get; set; }

        #region Labels
        private string terminalTitle;
        public string TerminalTitle => GetCachedValue(ref terminalTitle, Constants.Labels.Messages);

        private string filterTitle;
        public string FilterTitle => GetCachedValue(ref filterTitle, Constants.Labels.Filters);

        private string levelsTitle;
        public string LevelsTitle => GetCachedValue(ref levelsTitle, Constants.Labels.Levels);

        private string editTitle;
        public string EditTitle => GetCachedValue(ref editTitle, Constants.Labels.Edit);

        private string removeTitle;
        public string RemoveTitle => GetCachedValue(ref removeTitle, Constants.Labels.Remove);

        private string startRAMTitle;
        public string StartRAMTitle => GetCachedValue(ref startRAMTitle, Constants.Labels.StartRAM);

        private string startDiskTitle;
        public string StartDiskTitle => GetCachedValue(ref startDiskTitle, Constants.Labels.StartDisk);

        private string stopTitle;
        public string StopTitle => GetCachedValue(ref stopTitle, Constants.Labels.Stop);

        private string saveChangesTitle;
        public string SaveChangesTitle => GetCachedValue(ref saveChangesTitle, Constants.Labels.SaveChanges);
        #endregion

        #region Images
        public abstract string ComponentImage { get; }

        private string playDiskImage;
        public string PlayDiskImage => GetCachedValue(ref playDiskImage, $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}");

        private string playRAMImage;
        public string PlayRAMImage => GetCachedValue(ref playRAMImage, $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}");

        private string cancelImage;
        public string CancelImage => GetCachedValue(ref cancelImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}");

        private string removeImage;
        public string RemoveImage => GetCachedValue(ref removeImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageDelete}");

        private string editImage;
        public string EditImage => GetCachedValue(ref editImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageEdit}");

        private string saveImage;
        public string SaveImage => GetCachedValue(ref saveImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageSave}");

        private string terminalImage;
        public string TerminalImage => GetCachedValue(ref terminalImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageTerminal}");

        private string monitorImage;
        public string MonitorImage => GetCachedValue(ref monitorImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageMonitor}");

        private string filterImage;
        public string FilterImage => GetCachedValue(ref filterImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}");
        #endregion

        #region Visual properties
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        private StoreTypes storeType;
        public StoreTypes StoreType
        {
            get { return storeType; }
            set { storeType = value; NotifyPropertyChanged(); }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; NotifyPropertyChanged(); }
        }

        private string filterText;
        public string FilterText
        {
            get { return filterText; }
            set { filterText = value; NotifyPropertyChanged(); }
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
            set { allowChanges = value; NotifyPropertyChanged(); }
        }

        private int visibleMessagesNr = Constants.Component.DefaultRows;
        public int VisibleMessagesNr
        {
            get { return visibleMessagesNr; }
            set { visibleMessagesNr = value; NotifyPropertyChanged(); }
        }

        public HashSet<LogEventsVM> ConsoleMessages { get; set; } = new HashSet<LogEventsVM>();
        public ObservableCollection<LogEventsVM> VisibleConsoleMessages { get; set; } = new ObservableCollection<LogEventsVM>();
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
            ComponentLevels = new Dictionary<LevelTypes, LevelsVM>
            {
                { LevelTypes.All, new LevelsVM(LevelTypes.All) },
                { LevelTypes.Verbose, new LevelsVM(LevelTypes.Verbose) },
                { LevelTypes.Debug, new LevelsVM(LevelTypes.Debug) },
                { LevelTypes.Information, new LevelsVM(LevelTypes.Information) },
                { LevelTypes.Warning, new LevelsVM(LevelTypes.Warning) },
                { LevelTypes.Error, new LevelsVM(LevelTypes.Error) },
                { LevelTypes.Fatal, new LevelsVM(LevelTypes.Fatal) }
            };
            ComponentLevels[LevelTypes.All].IsSelected = true;
            IsAllSelected = true;

            // set level counters
            MessageContainer.Disk.ComponentCounters.Add(ComponentRegisterName, new ObservableCounterDictionary<LevelTypes>
            {
                { LevelTypes.All, 0 },
                { LevelTypes.Verbose, 0},
                { LevelTypes.Debug, 0 },
                { LevelTypes.Information, 0 },
                { LevelTypes.Warning, 0 },
                { LevelTypes.Error, 0 },
                { LevelTypes.Fatal, 0 }
            });

            // Set cleanup command 
            CleanUpCommand = new RelayCommand<bool>((canClean) =>
            {
                if (!canClean) { return; }

                // Clear messages
                ConsoleMessages.Clear();
                VisibleConsoleMessages.Clear();

                // clear database
                DbProcessor.CleanDatabase(ComponentRegisterName);

                // Clear counters
                MessageContainer.Disk.ComponentCounters[ComponentRegisterName].ResetAllCounters(fireChangedEvent: false);

                // Clear visible counters 
                foreach (var lvlCounter in ComponentLevels)
                {
                    lvlCounter.Value.Counter = 0;
                }
            });

            // set start listener/reader
            StartDiskListenerCommand = new RelayCommand(() => StartAction(StoreTypes.Disk));
            StartRAMListenerCommand = new RelayCommand(() => StartAction(StoreTypes.RAM));

            // Set stop listener/reader command 
            StopListenerCommand = new RelayCommand(() =>
            {
                try
                {
                    IsRunning = false;
                    if (asyncWorker.IsBusy)
                    {
                        Task cancelTsk = Task.Run(() => StopListener());
                        cancelTsk.Wait();
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
            });

            // Set filter search command 
            FilterTextSearchCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    FilterMessages();
                });
            });

            // Set filter clear command 
            FilterTextClearCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    FilterText = string.Empty;
                    FilterMessages();
                });
            });

            // Set levels filter command
            FilterLevelCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    if (IsAllSelected && ComponentLevels[LevelTypes.All].IsSelected && ComponentLevels.Values.Any(x => x.LevelType != LevelTypes.All && x.IsSelected))
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
                });
            });
        }

        private void StartAction(StoreTypes storeType)
        {
            try
            {
                IsRunning = true;
                StoreType = storeType;
                ProcessorMonitorContainer.ComponentStopper[ComponentRegisterName] = false;

                if (!asyncWorker.IsBusy)
                {
                    // Clear previous entries 
                    CleanUpCommand.Execute(true);

                    // Set background worker
                    InitializeBackWorker();
                    asyncWorker.RunWorkerAsync();
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
        }

        protected void FilterMessages()
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                bool hasChanges = false;
                var selectedLevels = ComponentLevels.Values.Where(x => x.IsSelected).Select(x => x.LevelType).ToList();

                if (SelectionFilters == null || SelectionFilters.FilterText != FilterText || !SelectionFilters.SameLevels(selectedLevels))
                {
                    // update current hash
                    hasChanges = true;
                    SelectionFilters = new SelectionElements { FilterText = this.FilterText, Levels = selectedLevels };
                }

                if (!hasChanges && VisibleConsoleMessages.Count == VisibleMessagesNr) { return; }

                // get entries from RAM or Disk
                if (StoreType == StoreTypes.Disk)
                {
                    FilterEntriesDisk(hasChanges, selectedLevels);
                }
                else
                {
                    FilterEntriesRAM(hasChanges, selectedLevels);
                }
            });
        }

        private void FilterEntriesDisk(bool hasChanges, List<LevelTypes> selectedLevels)
        {
            IList<LogEventsVM> filteredEntries = null;

            if (ComponentLevels[LevelTypes.All].IsSelected)
            {
                if (String.IsNullOrEmpty(FilterText))
                {
                    filteredEntries = DbProcessor.ReadAll(ComponentRegisterName, VisibleMessagesNr);
                }
                else
                {
                    filteredEntries = DbProcessor.Read(ComponentRegisterName, x => x.RenderedMessage.ToLower().Contains(FilterText.ToLower()), VisibleMessagesNr);
                }

            }
            else
            {
                Func<Entry, bool> predicate;
                if (String.IsNullOrEmpty(FilterText))
                {
                    predicate = x => selectedLevels.Contains((LevelTypes)x.LevelType);
                }
                else
                {
                    predicate = x => selectedLevels.Contains((LevelTypes)x.LevelType) && x.RenderedMessage.ToLower().Contains(FilterText.ToLower());

                }
                filteredEntries = DbProcessor.Read(ComponentRegisterName, predicate, VisibleMessagesNr);
            }

            // add filtered entries
            AddFilteredEntries(filteredEntries, () => hasChanges || filteredEntries.Count() != VisibleConsoleMessages.Count);
        }

        private void FilterEntriesRAM(bool hasChanges, List<LevelTypes> selectedLevels)
        {
            IEnumerable<LogEventsVM> filteredEntries = ConsoleMessages.AsEnumerable();

            // filter level
            if (!ComponentLevels[LevelTypes.All].IsSelected)
            {
                filteredEntries = filteredEntries.Where(x => selectedLevels.Contains(x.LevelType));
            }

            // filter text
            if (!String.IsNullOrEmpty(FilterText))
            {
                filteredEntries = filteredEntries.Where(x => x.RenderedMessage.ToLower().Contains(FilterText.ToLower()));
            }

            // filter visible rows
            filteredEntries = filteredEntries.Take(VisibleMessagesNr);

            // add filtered entries
            AddFilteredEntries(filteredEntries, () => hasChanges || filteredEntries.Count() != VisibleConsoleMessages.Count);
        }

        private void AddFilteredEntries(IEnumerable<LogEventsVM> filteredEntries, Func<bool> predicate)
        {
            if (predicate.Invoke())
            {
                // clear visible messages
                VisibleConsoleMessages.Clear();

                foreach (var entry in filteredEntries)
                {
                    // add item to console messages 
                    VisibleConsoleMessages.Add(entry);
                }
            }
        }

        protected void StopListener()
        {
            asyncWorker.CancelAsync();
            ProcessorMonitorContainer.ComponentStopper[ComponentRegisterName] = true;
        }

        protected void PlaySound()
        {
            SystemSounds.Hand.Play();
        }

        protected abstract void InitializeBackWorker();
        public abstract void RemoveComponent();
        public abstract void ClearComponent();

        private class SelectionElements
        {
            public string FilterText { get; set; }
            public List<LevelTypes> Levels { get; set; }

            public bool SameLevels(IEnumerable<LevelTypes> levels)
            {
                return levels.SequenceEqual(Levels);
            }
        }
    }
}
