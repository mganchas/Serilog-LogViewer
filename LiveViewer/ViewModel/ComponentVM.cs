using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Configs;
using LiveViewer.Model;
using LiveViewer.Services;
using LiveViewer.Types;
using static LiveViewer.Types.Levels;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public abstract class ComponentVM : BaseVM
    {
        public ComponentVM Self => this;
        protected BackgroundWorker asyncWorker = new BackgroundWorker();
        protected Timer timer;
        protected string ComponentRegisterName => $"{Name.Replace(' ', '_')}";
        private bool IsAllSelected { get; set; }

        #region Visual properties
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        private string path = Constants.Component.DefaultHttpPath;
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
                NotifyPropertyChanged(nameof(StartStopButtonImage));
                NotifyPropertyChanged(nameof(AllowChanges));
            }
        }

        private bool allowChanges = true;
        public bool AllowChanges
        {
            get { return allowChanges; }
            set { allowChanges = value; NotifyPropertyChanged(); }
        }

        private bool editMode = false;
        public bool EditMode
        {
            get { return editMode; }
            set
            {
                editMode = value;
                ReadMode = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ReadMode));
            }
        }

        private bool readMode = true;
        public bool ReadMode
        {
            get { return readMode; }
            set { readMode = value; NotifyPropertyChanged(); }
        }

        private string startStopButtonImage;
        public string StartStopButtonImage
        {
            get
            {
                if (String.IsNullOrEmpty(startStopButtonImage))
                {
                    startStopButtonImage = SearchImage;
                }
                else
                {
                    startStopButtonImage = IsRunning ? CancelImage : SearchImage;
                }
                return startStopButtonImage;
            }
        }

        public Dictionary<LevelTypes, ObservableCollection<LogEventsVM>> ConsoleMessages { get; set; }
        public ObservableCollection<LogEventsVM> VisibleConsoleMessages { get; set; } = new ObservableCollection<LogEventsVM>();
        #endregion

        #region Labels
        public string TerminalTitle => Constants.Labels.Messages;
        public string FilterTitle => Constants.Labels.Filters;
        public string LevelsTitle => Constants.Labels.Levels;
        public string EditTitle => Constants.Labels.Edit;
        public string RemoveTitle => Constants.Labels.Remove;
        public string StartStopTitle => Constants.Labels.StartStop;
        public string SaveChangesTitle => Constants.Labels.SaveChanges;
        #endregion

        #region Images
        public abstract string ComponentImage { get; }
        public abstract string SearchImage { get; }
        public abstract string CancelImage { get; }
        public string RemoveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageDelete}";
        public string EditImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageEdit}";
        public string SaveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageSave}";
        public string TerminalImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageTerminal}";
        public string MonitorImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageMonitor}";
        public string FilterImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFilter}";
        #endregion

        #region Levels
        public Dictionary<LevelTypes, LevelsVM> ComponentLevels { get; set; }
        #endregion        

        #region Commands
        public ICommand StartStopListenerCommand { get; set; }
        public ICommand CleanUpCommand { get; set; }
        public ICommand EditComponentCommand { get; set; }
        public ICommand RemoveComponentCommand { get; set; }
        public ICommand SaveChangesCommand { get; set; }
        public ICommand FilterTextChangedCommand { get; set; }
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

            // Set messages levels 
            ConsoleMessages = new Dictionary<LevelTypes, ObservableCollection<LogEventsVM>>
            {
                { LevelTypes.All, new ObservableCollection<LogEventsVM>() },
                { LevelTypes.Verbose, new ObservableCollection<LogEventsVM>() },
                { LevelTypes.Debug, new ObservableCollection<LogEventsVM>() },
                { LevelTypes.Information, new ObservableCollection<LogEventsVM>() },
                { LevelTypes.Warning, new ObservableCollection<LogEventsVM>() },
                { LevelTypes.Error, new ObservableCollection<LogEventsVM>() },
                { LevelTypes.Fatal, new ObservableCollection<LogEventsVM>() }
            };

            // Set cleanup command 
            CleanUpCommand = new RelayCommand<bool>((canClean) =>
            {
                if (!canClean) { return; }

                // Clear messages
                VisibleConsoleMessages.Clear();

                // Clear counters 
                foreach (var item in ComponentLevels)
                {
                    item.Value.Counter = 0;
                }
            });

            // Set edit component command 
            EditComponentCommand = new RelayCommand(() =>
            {
                EditMode = !EditMode;
            });

            // Set save changes command 
            SaveChangesCommand = new RelayCommand(() =>
            {
                EditMode = false;
            });

            // Set filter text changed command 
            FilterTextChangedCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    FilterMessages();
                });
            });

            // Set levels filter command
            FilterLevelCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    if (ComponentLevels.Values.Count(x => x.IsSelected) > 1 && ComponentLevels[LevelTypes.All].IsSelected && IsAllSelected)
                    {
                        ComponentLevels[LevelTypes.All].IsSelected = false;
                        IsAllSelected = false;
                    }

                    if (ComponentLevels[LevelTypes.All].IsSelected && !IsAllSelected)
                    {
                        foreach (var filterLevel in ComponentLevels)
                        {
                            filterLevel.Value.IsSelected = false;
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
                // reset visible collection 
                VisibleConsoleMessages.Clear();
                bool hasTextToFilter = !String.IsNullOrEmpty(FilterText);
                IEnumerable<Entry> dbLst;

                // filter with text 
                if (hasTextToFilter)
                {
                    dbLst = ComponentLevels[LevelTypes.All].IsSelected ?
                              new DbProcessor().GetAllEntriesWithText(ComponentRegisterName, FilterText) :
                              new DbProcessor().GetAllEntriesWithTextAndLevels(ComponentRegisterName, FilterText, ComponentLevels.Values.Where(x => x.IsSelected).Select(x => x.LevelType));
                }
                else
                {
                    dbLst = ComponentLevels[LevelTypes.All].IsSelected ?
                              new DbProcessor().GetAllEntries(ComponentRegisterName) :
                              new DbProcessor().GetAllEntriesWithLevels(ComponentRegisterName, ComponentLevels.Values.Where(x => x.IsSelected).Select(x => x.LevelType));
                }

                foreach (var entry in dbLst)
                {
                    // increment specific button counter 
                    ComponentLevels[entry.LevelType].Counter++;
                    ComponentLevels[Levels.LevelTypes.All].Counter++;

                    // add item to console messages 
                    VisibleConsoleMessages.Add(new LogEventsVM
                    {
                        Timestamp = entry.Timestamp,
                        RenderedMessage = entry.RenderedMessage,
                        LevelRaw = entry.LevelRaw,
                        LevelColor = Levels.GetLevelColor(entry.LevelType)
                    });
                }
            });
        }

        #region Abstract methods
        protected abstract void InitializeBackWorker();
        #endregion
    }
}
