using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Utils;
using Microsoft.Owin.Hosting;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public class ComponentVM : BaseVM
    {
        private readonly BackgroundWorker asyncWorker = new BackgroundWorker();

        public ComponentVM Self => this;

        #region Visual properties
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        private string httpPath = Constants.DefaultHttpPath;
        public string HttpPath
        {
            get { return httpPath; }
            set { httpPath = value; NotifyPropertyChanged(); }
        }

        private string httpRoute = Constants.DefaultHttpRoute;
        public string HttpRoute
        {
            get { return httpRoute; }
            set { httpRoute = value; NotifyPropertyChanged(); }
        }

        private bool isFiltered => !String.IsNullOrEmpty(FilterText);
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
        private static readonly string SearchImage = $"{Constants.ImagePath}{Constants.ImageSearch}";
        private static readonly string CancelImage = $"{Constants.ImagePath}{Constants.ImageCancel}";

        public string RemoveImage => $"{Constants.ImagePath}{Constants.ImageDelete}";
        public string ComponentImage => $"{Constants.ImagePath}{Constants.ImageComponent}";
        public string EditImage => $"{Constants.ImagePath}{Constants.ImageEdit}";
        public string TerminalImage => $"{Constants.ImagePath}{Constants.ImageTerminal}";
        public string MonitorImage => $"{Constants.ImagePath}{Constants.ImageMonitor}";
        public string FilterImage => $"{Constants.ImagePath}{Constants.ImageFilter}";
        #endregion

        #region Levels
        public Levels.LevelTypes CurrentLevel { get; set; } = Levels.LevelTypes.All;
        public LevelsVM AllLevel { get; set; }
        public LevelsVM DebugLevel { get; set; }
        public LevelsVM InformationLevel { get; set; }
        public LevelsVM WarningLevel { get; set; }
        public LevelsVM ErrorLevel { get; set; }
        public LevelsVM FatalLevel { get; set; }
        #endregion

        private string startStopButtonImage = SearchImage;
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

        public ComponentVM()
        {
            /* Set start/stop command */
            StartStopListenerCommand = new RelayCommand(() =>
            {
                IsRunning = !IsRunning;
                try
                {
                    if (asyncWorker.IsBusy)
                    {
                        asyncWorker.CancelAsync();
                    }
                    else
                    {
                        /* Set background worker */
                        InitializeBackWorker();
                        asyncWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception occurred: {ex.Message}");
                    asyncWorker.CancelAsync();
                }
            });

            /* Set edit component command */

            /* Set messages filter commands */
            AllLevel = new LevelsVM { LevelType = Levels.LevelTypes.All, TextColor = Levels.GetLevelColor(Levels.LevelTypes.All), ClickCommand = new RelayCommand(() => FilterMessages(Levels.LevelTypes.All)) };
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

            /* Set message collection onchanged event */
            MessageContainer.Messages.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null)
                {
                    try
                    {
                        foreach (LogEvent msg in e.NewItems)
                        {
                            var levelConverted = Levels.GetLevelTypeFromString(msg.Level);
                            msg.LevelColor = Levels.GetLevelColor(levelConverted);

                            App.Current.Dispatcher.Invoke(delegate
                            {
                                /* increment specific button counter */
                                switch (levelConverted)
                                {
                                    case Levels.LevelTypes.Debug:
                                        DebugLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Information:
                                        InformationLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Warning:
                                        WarningLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Error:
                                        ErrorLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Fatal:
                                        FatalLevel.Counter++;
                                        break;
                                    default:
                                        break;
                                }

                                /* increment all category */
                                AllLevel.Counter++;

                                /* add item to current level */
                                if (((CurrentLevel == Levels.LevelTypes.All) || (CurrentLevel == levelConverted)) &&
                                    (!isFiltered || (isFiltered && msg.RenderedMessage.ToLower().Contains(FilterText.ToLower()))))
                                {
                                    VisibleConsoleMessages.Add(msg);
                                }

                                /* add item to console messages */
                                ConsoleMessages.Add(msg);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            };

            #region Local functions
            void InitializeBackWorker()
            {
                asyncWorker.WorkerReportsProgress = true;
                asyncWorker.WorkerSupportsCancellation = true;
                asyncWorker.RunWorkerCompleted += delegate { if (IsRunning) IsRunning = false; };
                asyncWorker.DoWork += (sender, e) =>
                {
                    BackgroundWorker bwAsync = sender as BackgroundWorker;

                    try
                    {
                        using (WebApp.Start<Startup>(HttpPath))
                        {
                            while (!e.Cancel)
                            {
                                if (bwAsync.CancellationPending)
                                {
                                    e.Cancel = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                    }
                };

            }

            void FilterMessages(Levels.LevelTypes level, bool filterText = false)
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    /* reset visible collection */
                    VisibleConsoleMessages.Clear();

                    var levelStr = Levels.GetLevelStringFromType(level);
                    var consMsgs = ConsoleMessages.AsEnumerable();

                    /* apply filters */
                    if (level != Levels.LevelTypes.All) {
                        consMsgs = consMsgs.Where(x => x.Level == levelStr);
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
    }
}
