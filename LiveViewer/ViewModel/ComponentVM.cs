﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
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
        //protected Timer timer;
        protected string ComponentRegisterName => $"{Name.Replace(' ', '_')}";
        private bool IsAllSelected { get; set; }
        private HashElements SelectionFilters { get; set; }

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

        public HashSet<LogEventsVM> ConsoleMessages { get; set; } = new HashSet<LogEventsVM>();
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
                    if (ComponentLevels.Values.Any(x => x.LevelType != LevelTypes.All && x.IsSelected) && ComponentLevels[LevelTypes.All].IsSelected && IsAllSelected)
                    {
                        ComponentLevels[LevelTypes.All].IsSelected = false;
                        IsAllSelected = false;
                    }

                    if (ComponentLevels[LevelTypes.All].IsSelected && !IsAllSelected)
                    {
                        foreach (var filterLevel in ComponentLevels)
                        {
                            if (filterLevel.Key != LevelTypes.All) {
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
                bool hasChanges = false;
                var selectedLevels = ComponentLevels.Values.Where(x => x.IsSelected).Select(x => x.LevelType).ToList();

                if (SelectionFilters == null || SelectionFilters.FilterText != FilterText || !SelectionFilters.SameLevels(selectedLevels))
                {
                    // update current hash
                    hasChanges = true;
                    SelectionFilters = new HashElements { FilterText = this.FilterText, Levels = selectedLevels };
                }

                if (hasChanges || VisibleConsoleMessages.Count < Constants.Component.DefaultRows)
                {
                    IEnumerable<LogEventsVM> filteredEntries = ConsoleMessages.AsEnumerable();

                    // clear visible messages
                    VisibleConsoleMessages.Clear();

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

                    foreach (var entry in filteredEntries)
                    {
                        // add item to console messages 
                        VisibleConsoleMessages.Add(new LogEventsVM
                        {
                            Timestamp = entry.Timestamp,
                            RenderedMessage = entry.RenderedMessage,
                            LevelType = entry.LevelType
                        });
                    }
                }
            });
        }

        protected void PlaySound()
        {
            SystemSounds.Exclamation.Play();
        }

        #region Abstract methods
        protected abstract void InitializeBackWorker();
        public abstract void RemoveComponent();
        public abstract void ClearComponent();
        #endregion
    }

    public class HashElements
    {
        public string FilterText { get; set; }
        public List<LevelTypes> Levels { get; set; }

        public bool SameLevels(IEnumerable<LevelTypes> levels)
        {
            return levels.SequenceEqual(Levels);
        }
    }
}
