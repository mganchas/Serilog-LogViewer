using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiveViewer.Utils;
using Microsoft.Owin.Hosting;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public class ComponentVM : BaseVM
    {
        private readonly BackgroundWorker asyncWorker = new BackgroundWorker();

        public string Name { get; set; }
        public string HttpPath { get; set; }
        public string HttpRoute { get; set; }

        #region Images
        private static readonly string SearchImage = $"{Constants.ImagePath}{Constants.ImageSearch}";
        private static readonly string CancelImage = $"{Constants.ImagePath}{Constants.ImageCancel}";

        public string RemoveImage => $"{Constants.ImagePath}{Constants.ImageDelete}";
        public string ComponentImage => $"{Constants.ImagePath}{Constants.ImageComponent}";
        public string EditImage => $"{Constants.ImagePath}{Constants.ImageEdit}";
        public string TerminalImage => $"{Constants.ImagePath}{Constants.ImageTerminal}";
        public string MonitorImage => $"{Constants.ImagePath}{Constants.ImageMonitor}";
        #endregion

        #region Collections
        public ObservableCollection<LogEvent> ConsoleMessages { get; set; } = new ObservableCollection<LogEvent>();
        public ObservableCollection<LogEvent> VisibleConsoleMessages { get; set; } = new ObservableCollection<LogEvent>();
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

        private bool allowChanged;
        public bool AllowChanges
        {
            get { return allowChanged; }
            set { allowChanged = value; NotifyPropertyChanged(); }
        }

        #region Commands
        public ICommand StartStopListenerCommand { get; set; }
        public ICommand CleanUpCommand { get; set; }
        public ICommand EditComponentCommand { get; set; }
        public ICommand RemoveComponentCommand { get; set; }
        #endregion

        public ComponentVM()
        {
            /* Set background worker */
            InitializeBackWorker();

            /* Set start/stop command */
            StartStopListenerCommand = new GenericCommand(() =>
            {
                IsRunning = !IsRunning;
                //try
                //{
                    //if (asyncWorker.IsBusy) {
                    //    asyncWorker.CancelAsync();
                    //}
                    //else {
                    //    asyncWorker.RunWorkerAsync();
                    //}
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show($"Exception occurred: {ex.Message}");
                //    asyncWorker.CancelAsync();
                //}
            });

            /* Set edit component command */

            /* Set remove component command */
            RemoveComponentCommand = new GenericCommand(() =>
            {
                
            });

            /* Set messages filter commands */
            AllLevel = new LevelsVM { LevelType = Levels.LevelTypes.All, TextColor = Levels.GetLevelColor(Levels.LevelTypes.All), ClickCommand = new GenericCommand(() => FilterMessages(Levels.LevelTypes.All)) };
            DebugLevel = new LevelsVM { LevelType = Levels.LevelTypes.Debug, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Debug), ClickCommand = new GenericCommand(() => FilterMessages(Levels.LevelTypes.Debug)) };
            InformationLevel = new LevelsVM { LevelType = Levels.LevelTypes.Information, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Information), ClickCommand = new GenericCommand(() => FilterMessages(Levels.LevelTypes.Information)) };
            WarningLevel = new LevelsVM { LevelType = Levels.LevelTypes.Warning, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Warning), ClickCommand = new GenericCommand(() => FilterMessages(Levels.LevelTypes.Warning)) };
            ErrorLevel = new LevelsVM { LevelType = Levels.LevelTypes.Error, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Error), ClickCommand = new GenericCommand(() => FilterMessages(Levels.LevelTypes.Error)) };
            FatalLevel = new LevelsVM { LevelType = Levels.LevelTypes.Fatal, TextColor = Levels.GetLevelColor(Levels.LevelTypes.Fatal), ClickCommand = new GenericCommand(() => FilterMessages(Levels.LevelTypes.Fatal)) };

            /* Set cleanup command */
            CleanUpCommand = new GenericCommand(() =>
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

            }, (s) => { return true; });

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
                                if ((CurrentLevel == Levels.LevelTypes.All) || (CurrentLevel == levelConverted))
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
                };
                
            }

            void FilterMessages(Levels.LevelTypes level)
            {
                App.Current.Dispatcher.Invoke(delegate
                {
                    /* reset visible collection */
                    VisibleConsoleMessages.Clear();

                    /* fill visible collection according to filter */
                    if (level == Levels.LevelTypes.All)
                    {
                        ConsoleMessages.ToList().ForEach(x => VisibleConsoleMessages.Add(x));
                    }
                    else
                    {
                        var levelStr = Levels.GetLevelStringFromType(level);
                        ConsoleMessages.Where(x => x.Level == levelStr).ToList().ForEach(x => VisibleConsoleMessages.Add(x));
                    }

                    /* set current selected level */
                    CurrentLevel = level;
                });
            }
            #endregion
        }
    }
}
