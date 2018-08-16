using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Utils;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using static LiveViewer.Utils.Levels;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public abstract class ComponentVM : BaseVM
    {
        public ComponentVM Self => this;
        protected BackgroundWorker asyncWorker = new BackgroundWorker();
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

        public Dictionary<LevelTypes, ObservableCollection<LogEvent>> ConsoleMessages { get; set; }
        public ObservableCollection<LogEvent> VisibleConsoleMessages { get; set; } = new ObservableCollection<LogEvent>();
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

            /* Set levels */
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

            /* Set messages levels */
            ConsoleMessages = new Dictionary<LevelTypes, ObservableCollection<LogEvent>>
            {
                { LevelTypes.All, new ObservableCollection<LogEvent>() },
                { LevelTypes.Verbose, new ObservableCollection<LogEvent>() },
                { LevelTypes.Debug, new ObservableCollection<LogEvent>() },
                { LevelTypes.Information, new ObservableCollection<LogEvent>() },
                { LevelTypes.Warning, new ObservableCollection<LogEvent>() },
                { LevelTypes.Error, new ObservableCollection<LogEvent>() },
                { LevelTypes.Fatal, new ObservableCollection<LogEvent>() }
            };

            /* Set cleanup command */
            CleanUpCommand = new RelayCommand<bool>((canClean) =>
            {
                if (!canClean) { return; }

                /* Specific clean actions */
                ClearComponent();

                /* Clear collections */
                foreach (var key in ConsoleMessages.Keys) {
                    ConsoleMessages[key].Clear();
                }
                VisibleConsoleMessages.Clear();

                /* Clear counters */
                foreach (var item in ComponentLevels) {
                    item.Value.Counter = 0;
                }
            });

            /* Set edit component command */
            EditComponentCommand = new RelayCommand(() =>
            {
                EditMode = !EditMode;
            });

            /* Set save changes command */
            SaveChangesCommand = new RelayCommand(() =>
            {
                EditMode = false;
            });

            /* Set filter text changed command */
            FilterTextChangedCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    FilterMessages();
                });
            });

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
                        foreach (var filterLevel in ComponentLevels) {
                            filterLevel.Value.IsSelected = false;
                        }
                        IsAllSelected = true;
                    }

                    FilterMessages();
                });
            });
        }

        void FilterMessages()
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                /* reset visible collection */
                VisibleConsoleMessages.Clear();
                bool hasTextToFilter = !String.IsNullOrEmpty(FilterText);

                foreach (var filterLevel in ComponentLevels.Values.Where(x => x.IsSelected))
                {
                    var msgs = ConsoleMessages[filterLevel.LevelType].AsEnumerable();

                    /* filter with text */
                    if (hasTextToFilter) {
                        msgs = msgs.Where(x => x.RenderedMessage.ToLower().Contains(FilterText.ToLower()));
                    }

                    foreach (var item in msgs)
                    {
                        VisibleConsoleMessages.Add(item);
                    }
                }

                //var consMsgs = ConsoleMessages.AsEnumerable();
                //var consMsgs = new List<Lookup<LevelTypes, LogEvent>>();

                ///* apply filters */
                //if (!levels.Contains(LevelTypes.All))
                //{
                //    foreach (var lvl in levels)
                //    {
                //        consMsgs.AddRange(ConsoleMessages[lvl].ToList());
                //    }
                //    //consMsgs = consMsgs.Where(x => levels.Contains(x.LevelType));
                //}

                //if (!String.IsNullOrEmpty(FilterText))
                //{
                //    //consMsgs = consMsgs.Where(x => x.RenderedMessage.ToLower().Contains(FilterText?.ToLower()));
                //}

                ///* filter observable list */
                //foreach (var msg in consMsgs) {
                //    VisibleConsoleMessages = new ObservableCollection<LogEvent>(consMsgs.ToList()); //.Add(msg);
                //}

                ///* set current selected level */
                //CurrentLevels = levels.ToList();
            });
        }

        #region Abstract methods
        protected abstract void InitializeBackWorker();
        public abstract void RemoveComponent();
        public abstract void ClearComponent();
        #endregion
    }
}
