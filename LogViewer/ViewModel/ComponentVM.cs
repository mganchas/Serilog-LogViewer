using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using static LogViewer.Model.Levels;

namespace LogViewer.ViewModel
{
    public abstract class ComponentVM : PropertyChangesNotifier, ICustomComponent
    {
        public ComponentVM Self => this;
        protected BackgroundWorker asyncWorker = new BackgroundWorker();
        protected string ComponentRegisterName => $"{Name.Replace(' ', '_')}";
        private bool IsAllSelected { get; set; }
        private SelectionElements SelectionFilters { get; set; }

        #region Labels
        public string TerminalTitle => Constants.Labels.Messages;
        public string FilterTitle => Constants.Labels.Filters;
        public string LevelsTitle => Constants.Labels.Levels;
        public string EditTitle => Constants.Labels.Edit;
        public string RemoveTitle => Constants.Labels.Remove;
        public string StartRAMTitle => Constants.Labels.StartRAM;
        public string StartDiskTitle => Constants.Labels.StartDisk;
        public string StopTitle => Constants.Labels.Stop;
        public string SaveChangesTitle => Constants.Labels.SaveChanges;
        #endregion

        #region Images
        public abstract string ComponentImage { get; }
        public string PlayDiskImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public string PlayRAMImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}";
        public string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        public string RemoveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageDelete}";
        public string EditImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageEdit}";
        public string SaveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageSave}";
        public string TerminalImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageTerminal}";
        public string MonitorImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageMonitor}";
        public string FilterImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFilter}";
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

        protected bool IsFiltered => !String.IsNullOrEmpty(FilterText);
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

        public HashSet<LogEventsVM> ConsoleMessages { get; set; } = new HashSet<LogEventsVM>();
        public ObservableCollection<LogEventsVM> VisibleConsoleMessages { get; set; } = new ObservableCollection<LogEventsVM>();
        #endregion

        #region Levels
        public Dictionary<LevelTypes, LevelsVM> ComponentLevels { get; set; }
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

        protected ComponentVM(string name, string path)
        {
            this.Name = name;
            this.Path = path;

            // Set levels 
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

            // Set cleanup command 
            CleanUpCommand = new RelayCommand<bool>((canClean) =>
            {
                if (!canClean) { return; }

                // Clear messages
                ConsoleMessages.Clear();
                VisibleConsoleMessages.Clear();

                // Clear counters 
                foreach (var item in ComponentLevels)
                {
                    item.Value.Counter = 0;
                }
            });

            // set start (disk) listener/reader
            StartDiskListenerCommand = new RelayCommand(() =>
            {
                try
                {
                    IsRunning = true;
                    StoreType = StoreTypes.Disk;

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
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            });

            // set start (ram) listener/reader
            StartRAMListenerCommand = new RelayCommand(() =>
            {
                try
                {
                    IsRunning = true;
                    StoreType = StoreTypes.RAM;

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
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            });

            // Set stop listener/reader command 
            StopListenerCommand = new RelayCommand(() =>
            {
                try
                {
                    IsRunning = false;
                    if (asyncWorker.IsBusy)
                    {
                        Task cancelTsk = Task.Run(() => asyncWorker.CancelAsync());
                        cancelTsk.Wait();
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
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
                    if (ComponentLevels.Values.Any(x => x.LevelType != LevelTypes.All && x.IsSelected) && ComponentLevels[LevelTypes.All].IsSelected && IsAllSelected)
                    {
                        ComponentLevels[LevelTypes.All].IsSelected = false;
                        IsAllSelected = false;
                    }

                    if (ComponentLevels[LevelTypes.All].IsSelected && !IsAllSelected)
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

        protected void FilterMessages()
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                int prevRows = VisibleConsoleMessages.Count;
                bool hasChanges = false;
                var selectedLevels = ComponentLevels.Values.Where(x => x.IsSelected).Select(x => x.LevelType).ToList();

                if (SelectionFilters == null || SelectionFilters.FilterText != FilterText || !SelectionFilters.SameLevels(selectedLevels))
                {
                    // update current hash
                    hasChanges = true;
                    SelectionFilters = new SelectionElements { FilterText = this.FilterText, Levels = selectedLevels };
                }

                if (hasChanges || VisibleConsoleMessages.Count < Constants.Component.DefaultRows)
                {
                    // get entries from RAM or Disk

                    if (StoreType == StoreTypes.Disk)
                    {
                        IList<Entry> filteredEntries = null;

                        // filter level
                        if (!ComponentLevels[LevelTypes.All].IsSelected && !String.IsNullOrEmpty(FilterText))
                        {
                            filteredEntries = DbProcessor.Read(x => selectedLevels.Contains((LevelTypes)x.LevelType) && x.RenderedMessage.ToLower().Contains(FilterText.ToLower()), Constants.Component.DefaultRows);
                        }
                        if (ComponentLevels[LevelTypes.All].IsSelected && !String.IsNullOrEmpty(FilterText))
                        {
                            filteredEntries = DbProcessor.Read(x => x.RenderedMessage.ToLower().Contains(FilterText.ToLower()), Constants.Component.DefaultRows);
                        }
                        else if (!ComponentLevels[LevelTypes.All].IsSelected && String.IsNullOrEmpty(FilterText))
                        {
                            filteredEntries = DbProcessor.Read(x => selectedLevels.Contains((LevelTypes)x.LevelType), Constants.Component.DefaultRows);
                        }
                        else
                        {
                            filteredEntries = DbProcessor.ReadAll(Constants.Component.DefaultRows);
                        }

                        if (hasChanges || filteredEntries.Count != prevRows)
                        {
                            // clear visible messages
                            VisibleConsoleMessages.Clear();

                            foreach (var entry in filteredEntries)
                            {
                                // add item to console messages 
                                VisibleConsoleMessages.Add(new LogEventsVM
                                {
                                    RenderedMessage = entry.RenderedMessage,
                                    Timestamp = entry.Timestamp,
                                    LevelType = (LevelTypes)entry.LevelType
                                });
                            }
                        }
                    }
                    else
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
                        filteredEntries = filteredEntries.Take(Constants.Component.DefaultRows);

                        if (hasChanges || filteredEntries.Count() != prevRows)
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
                }
            });
        }

        protected void PlaySound()
        {
            SystemSounds.Hand.Play();
        }

        #region Abstract methods
        protected abstract void InitializeBackWorker();
        public abstract void RemoveComponent();
        public abstract void ClearComponent();
        #endregion
    }

    public class SelectionElements
    {
        public string FilterText { get; set; }
        public List<LevelTypes> Levels { get; set; }

        public bool SameLevels(IEnumerable<LevelTypes> levels)
        {
            return levels.SequenceEqual(Levels);
        }
    }

    public static class ComponentFactory<T>
    {
        private const string MethodName = "IsValidComponent";
        public static T GetComponent<T>(object context, string name, string path, in ObservableCollection<ComponentVM> components, Func<string, string, T> creator) where T : ICustomComponent
        {
            T retObj = default;
            bool isValid = Convert.ToBoolean(typeof(T).GetMethod(MethodName).Invoke(context, new object[] { name, path, components }));
            if (isValid)
            {
                retObj = creator.Invoke(name, path);
            }
            return retObj;
        }
    }
}
