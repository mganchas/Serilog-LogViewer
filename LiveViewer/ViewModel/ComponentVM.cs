using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Utils;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public abstract class ComponentVM : BaseVM
    {
        public ComponentVM Self => this;
        protected BackgroundWorker asyncWorker = new BackgroundWorker();
        private readonly ComponentTypes componentType;

        #region Visual properties
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        protected string transfPath => !Path.EndsWith("/") ? $"{Path}/" : Path;
        private string path = Constants.Component.DefaultHttpPath;
        public string Path
        {
            get { return path; }
            set { path = value; NotifyPropertyChanged(); }
        }

        protected string transfHttpRoute => !HttpRoute.EndsWith("/") ? $"{HttpRoute}/" : HttpRoute;
        private string httpRoute = Constants.Component.DefaultHttpRoute;
        public string HttpRoute
        {
            get { return httpRoute; }
            set { httpRoute = value; NotifyPropertyChanged(); }
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
                NotifyPropertyChanged(nameof(startStopButtonImage));
                NotifyPropertyChanged(nameof(allowChanged));
            }
        }

        private bool allowChanged = true;
        public bool AllowChanges
        {
            get { return allowChanged; }
            set { allowChanged = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<LogEvent> ConsoleMessages { get; set; } = new ObservableCollection<LogEvent>();
        public ObservableCollection<LogEvent> VisibleConsoleMessages { get; set; } = new ObservableCollection<LogEvent>();
        #endregion

        #region Labels
        public string TerminalTitle => Constants.Labels.Messages;
        public string FilterTitle => Constants.Labels.Filters;
        public string LevelsTitle => Constants.Labels.Levels;
        #endregion

        #region Images
        private readonly string SearchImage = $"{Constants.Images.ImagePath}{Constants.Images.ImageSearch}";
        private readonly string CancelImage = $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";

        public string RemoveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageDelete}";
        public abstract string ComponentImage { get; }
        public string EditImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageEdit}";
        public string TerminalImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageTerminal}";
        public string MonitorImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageMonitor}";
        public string FilterImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFilter}";
        #endregion

        #region Levels
        public Levels.LevelTypes CurrentLevel { get; set; } = Levels.LevelTypes.All;
        public LevelsVM AllLevel { get; set; }
        public LevelsVM VerboseLevel { get; set; }
        public LevelsVM DebugLevel { get; set; }
        public LevelsVM InformationLevel { get; set; }
        public LevelsVM WarningLevel { get; set; }
        public LevelsVM ErrorLevel { get; set; }
        public LevelsVM FatalLevel { get; set; }
        #endregion

        private string startStopButtonImage = $"{Constants.Images.ImagePath}{Constants.Images.ImageSearch}";
        public string StartStopButtonImage
        {
            get
            {
                if (IsRunning)
                {
                    if (startStopButtonImage == SearchImage)
                    {
                        startStopButtonImage = CancelImage;
                        NotifyPropertyChanged();
                    }
                }
                else
                {
                    if (startStopButtonImage == CancelImage)
                    {
                        startStopButtonImage = SearchImage;
                        NotifyPropertyChanged();
                    }
                }
                return startStopButtonImage;
            }
        }

        #region Commands
        public ICommand StartStopListenerCommand { get; set; }
        public ICommand CleanUpCommand { get; set; }
        public ICommand EditComponentCommand { get; set; }
        public ICommand RemoveComponentCommand { get; set; }
        public ICommand FilterTextChangedCommand { get; set; }
        #endregion

        protected ComponentVM(ComponentTypes componentType, string name)
        {
            this.Name = name;
            this.componentType = componentType;

            /* Set messages filter commands */
            AllLevel = new LevelsVM { LevelType = Levels.LevelTypes.All, TextColor = Levels.GetLevelColor(Levels.LevelTypes.All), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.All)) };
            VerboseLevel = new LevelsVM { LevelType = Levels.LevelTypes.Verbose, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Verbose), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.Verbose)) };
            DebugLevel = new LevelsVM { LevelType = Levels.LevelTypes.Debug, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Debug), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.Debug)) };
            InformationLevel = new LevelsVM { LevelType = Levels.LevelTypes.Information, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Information), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.Information)) };
            WarningLevel = new LevelsVM { LevelType = Levels.LevelTypes.Warning, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Warning), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.Warning)) };
            ErrorLevel = new LevelsVM { LevelType = Levels.LevelTypes.Error, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Error), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.Error)) };
            FatalLevel = new LevelsVM { LevelType = Levels.LevelTypes.Fatal, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Fatal), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.Fatal)) };

            /* Set cleanup command */
            CleanUpCommand = new RelayCommand(() =>
            {
                /* Clear collections */
                ConsoleMessages.Clear();
                VisibleConsoleMessages.Clear();

                /* Clear counters */
                AllLevel.Counter = 0;
                VerboseLevel.Counter = 0;
                DebugLevel.Counter = 0;
                InformationLevel.Counter = 0;
                WarningLevel.Counter = 0;
                ErrorLevel.Counter = 0;
                FatalLevel.Counter = 0;

            });

            /* Set filter text changed command */
            FilterTextChangedCommand = new RelayCommand(() =>
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    FilterMessages(CurrentLevel, filterText: !String.IsNullOrEmpty(FilterText));
                });
            });

            #region Local functions
            void FilterMessages(Levels.LevelTypes level, bool filterText = false)
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    /* reset visible collection */
                    VisibleConsoleMessages.Clear();

                    var consMsgs = ConsoleMessages.AsEnumerable();

                    /* apply filters */
                    if (level != Levels.LevelTypes.All) {
                        consMsgs = consMsgs.Where(x => x.LevelType == level);
                    }
                    if (filterText) {
                        consMsgs = consMsgs.Where(x => x.RenderedMessage.ToLower().Contains(FilterText?.ToLower()));
                    }

                    /* filter observable list */
                    consMsgs.ToList().ForEach(x => VisibleConsoleMessages.Add(x));

                    /* set current selected level */
                    CurrentLevel = level;
                });
            }
            #endregion
        }

        protected abstract void InitializeBackWorker();
    }
}
