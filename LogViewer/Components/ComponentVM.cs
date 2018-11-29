using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LogViewer.Abstractions;
using LogViewer.Containers;
using LogViewer.Entries;
using LogViewer.Factories;
using LogViewer.Listeners;
using LogViewer.Model;
using LogViewer.Processors;
using LogViewer.Resources;
using LogViewer.Types;
using LogViewer.Utilities;
using LogViewer.View;

namespace LogViewer.Components
{
    public sealed class ComponentVM : PropertyChangesNotifier, ICustomComponent
    {
        public ComponentVM Self => this;
        public string Name { get; }
        public string Path { get; }

        private string ComponentRegisterName => $"{Name.Replace(' ', '_')}";
        private ComponentTypes ComponentType { get; set; }
        private bool IsAllSelected { get; set; } = true;
        private ComponentFiltersVM SelectionFilters { get; set; }
        private StoreTypes StoreType { get; set; }
        private IDbProcessor DbProcessor { get; set; }
        private IComponentProcessor ComponentProcessor { get; set; }

        private HashSet<LogEventsVM> _visibleConsoleMessages = new HashSet<LogEventsVM>();
        public HashSet<LogEventsVM> VisibleConsoleMessages
        {
            get => _visibleConsoleMessages;
            set
            {
                _visibleConsoleMessages = value;
                NotifyPropertyChanged();
            }
        }

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                AllowChanges = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(AllowChanges));
            }
        }

        private bool _canExport = true;
        public bool CanExport
        {
            get => _canExport;
            set
            {
                _canExport = value;
                NotifyPropertyChanged();
            }
        }

        private bool _allowChanges = true;
        public bool AllowChanges
        {
            get => _allowChanges;
            set
            {
                _allowChanges = value;
                NotifyPropertyChanged();
            }
        }

        private int _visibleMessagesNr = Constants.Component.DefaultRows;
        public int VisibleMessagesNr
        {
            get => _visibleMessagesNr;
            set
            {
                _visibleMessagesNr = value;
                NotifyPropertyChanged();
            }
        }

        //public List<LogEventsVM> ConsoleMessages { get; }
        public IEnumerable<IStoreProcessor> StoreProcessors { get; set; }
        public Dictionary<LevelTypes, LevelsVM> ComponentLevels { get; }

        #region Labels

        public static string TerminalTitle => Constants.Labels.Messages;
        public static string FilterTitle => Constants.Labels.Filters;
        public static string LevelsTitle => Constants.Labels.Levels;
        public static string EditTitle => Constants.Labels.Edit;
        public static string RemoveTitle => Constants.Labels.Remove;
        public static string StartTitle => Constants.Labels.Start;
        public static string StopTitle => Constants.Labels.Stop;
        public static string SaveChangesTitle => Constants.Labels.SaveChanges;
        public static string ExportContent => Constants.Tooltips.ExportContent;

        #endregion

        #region Images

        private string _componentImage;

        public string ComponentImage
        {
            get
            {
                if (string.IsNullOrEmpty(_componentImage))
                {
                    _componentImage = ComponentType.GetComponentImage();
                    NotifyPropertyChanged();
                }

                return _componentImage;
            }
        }

        public static string PlayImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}";
        public static string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        public static string RemoveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageDelete}";
        public static string EditImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageEdit}";
        public static string SaveImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageSave}";
        public static string TerminalImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageTerminal}";
        public static string MonitorImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageMonitor}";
        public static string FilterImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}";
        public static string ExportContentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageExpand}";

        #endregion

        #region Commands

        public IEnrichedCommand StopListenerCommand { get; set; }
        public IEnrichedCommand CleanUpCommand { get; set; }
        public IEnrichedCommand RemoveComponentCommand { get; set; }
        public IEnrichedCommand FilterTextSearchCommand { get; set; }
        public IEnrichedCommand FilterTextClearCommand { get; set; }
        public IEnrichedCommand FilterLevelCommand { get; set; }
        public IEnrichedCommand ExportContentCommand { get; set; }
        private IEnrichedCommand DefaultStartListenerCommand { get; set;  }
        #endregion

        public ComponentVM(string name, string path, ComponentTypes componentType)
        {
            Name = name;
            Path = path;
            ComponentType = componentType;
            
            RegisterComponent();
            
            ComponentLevels = LevelsVM.LevelVMList;
            StoreProcessors = StoreProcessorsHelper.GetStoreProcessorsList(DefaultStartListenerCommand);
        }

        private void RegisterComponent()
        {
            // Set component specific processor
            ComponentProcessor = ComponentType.GetComponentProcessor();

            // Add component to processor monitor
            ProcessorMonitorContainer.ComponentStopper.Add(ComponentRegisterName, false);

            // Register commands' behaviour
            RegisterDefaultStartCommand();
            RegisterStopCommand();
            RegisterCleanCommand();
            RegisterFilterTextSearchCommand();
            RegisterFilterTextClearCommand();
            RegisterFilterLevelCommand();
            RegisterExportContentCommand();
        }

        private void RegisterDefaultStartCommand()
        {
            DefaultStartListenerCommand = new CommandHandler((executionParameter) =>
            {
                StoreType = (StoreTypes) executionParameter;
                DbProcessor = StoreTypeProcessorFactory.GetStoreProcessor(StoreType);

                if (StoreType == StoreTypes.Local)
                {
                    // set level counters
                    if (MessageContainer.Counters.ContainsKey(ComponentRegisterName)) {
                        MessageContainer.Counters.Remove(ComponentRegisterName);
                    }
                    MessageContainer.Counters.Add(ComponentRegisterName, LevelsVM.LevelTypesCounterList);
                        
                    // Add new message queue
                    if (MessageContainer.Messages.ContainsKey(ComponentRegisterName)) {
                        MessageContainer.Messages.Remove(ComponentRegisterName);
                    }
                    MessageContainer.Messages.Add(ComponentRegisterName, new Lazy<ObservableSet<IEntry>>());
                    
                    var handler = new NotifyCollectionChangedEventHandler((sender, e) => { CollectionChanged(e, sender as ObservableSet<IEntry>); });
                    // Remove if there was already a handler
                    MessageContainer.Messages[ComponentRegisterName].Value.CollectionChanged -= handler;
                    // Register collection changed event
                    MessageContainer.Messages[ComponentRegisterName].Value.CollectionChanged += handler;
                }
                
                StartAction();
            });
        }

        private void RegisterStopCommand()
        {
            StopListenerCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsRunning = false;
                    StopListener();
                });
            });
        }

        private void RegisterCleanCommand()
        {
            CleanUpCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) ClearComponent);
            });
        }

        private void RegisterFilterTextSearchCommand()
        {
            FilterTextSearchCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) FilterMessages);
            });
        }

        private void RegisterFilterTextClearCommand()
        {
            FilterTextClearCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
                {
                    FilterText = string.Empty;
                    FilterMessages();
                }));
            });
        }

        private void RegisterFilterLevelCommand()
        {
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

        private void RegisterExportContentCommand()
        {
            ExportContentCommand = new CommandHandler(_ => { new VisibleMessagesWindow(CopyComponent(this)).Show(); });
        }

        private void CollectionChanged(NotifyCollectionChangedEventArgs e, ObservableSet<IEntry> originalSet)
        {
            if (!IsRunning || e.NewItems == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
            {
                // increment button counters
                foreach (IEntry entry in e.NewItems)
                {
                    ComponentLevels[(LevelTypes) entry.LevelType].Counter++;
                }

                ComponentLevels[LevelTypes.All].Counter += e.NewItems.Count;
            }));

            FilterMessages();
        }

        private void StartAction()
        {
            try
            {
                IsRunning = true;
                ProcessorMonitorContainer.ComponentStopper[ComponentRegisterName] = false;

                // Clear previous entries 
                CleanUpCommand.Execute();

                // Create background thread
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    ComponentProcessor.ReadData(Path, Name, DbProcessor);
                }).Start();
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
            var hasChanges = false;
            var currFilters = new ComponentFiltersVM
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
            if (StoreType == StoreTypes.MongoDB)
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
                filteredEntries = string.IsNullOrEmpty(FilterText)
                    ? new MongoDBProcessor(ComponentRegisterName).ReadAll<Entry>(ComponentRegisterName, VisibleMessagesNr)
                    : new MongoDBProcessor(ComponentRegisterName).ReadText<Entry>(ComponentRegisterName, FilterText.ToLower(), VisibleMessagesNr);
            }
            else
            {
                filteredEntries = string.IsNullOrEmpty(FilterText)
                    ? new MongoDBProcessor(ComponentRegisterName).ReadLevels<Entry>(ComponentRegisterName, selectedLevels)
                    : new MongoDBProcessor(ComponentRegisterName).ReadLevelsAndText<Entry>(ComponentRegisterName, selectedLevels, FilterText.ToLower());
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
            var filteredEntries = new List<LogEventsVM>().AsEnumerable();

            // filter level
            if (!ComponentLevels[LevelTypes.All].IsSelected)
            {
                filteredEntries = filteredEntries.Where(x => selectedLevels.Contains(x.LevelType));
            }

            // filter text
            if (!string.IsNullOrEmpty(FilterText))
            {
                var orFilters = Regex.Split(_filterText, @"\|");
                var andFilters = Regex.Split(_filterText, @"&+");

                if (orFilters.Length == 1 && andFilters.Length == 1)
                {
                    filteredEntries = filteredEntries
                        .Where(x => x.RenderedMessage.ToLower().Contains(TransformFilterString(FilterText)));
                }
                else if (orFilters.Length > 1)
                {
                    // process OR's
                    Func<LogEventsVM, bool> predicate = x => false;
                    foreach (var or in orFilters.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        var oldPredicate = predicate;
                        predicate = x =>
                            oldPredicate(x) || x.RenderedMessage.ToLower().Contains(TransformFilterString(or));
                    }

                    filteredEntries = filteredEntries.Where(predicate);
                }
                else if (andFilters.Length > 1)
                {
                    // process AND's
                    filteredEntries = andFilters.Aggregate(filteredEntries, (current, filter) =>
                        current.Where(x => x.RenderedMessage.ToLower().Contains(TransformFilterString(filter))));
                }
            }

            // filter visible rows
            filteredEntries = filteredEntries.Take(VisibleMessagesNr);

            // add filtered entries
            Application.Current.Dispatcher.Invoke(() =>
            {
                VisibleConsoleMessages = new HashSet<LogEventsVM>(filteredEntries);
            });
        }

        private void StopListener()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
                (Action) (() =>
                {
                    IsRunning = false;
                    ProcessorMonitorContainer.ComponentStopper[ComponentRegisterName] = true;
                }));
        }

        public void RemoveComponent()
        {
            MessageContainer.Messages.Remove(ComponentRegisterName);
            MessageContainer.Counters.Remove(ComponentRegisterName);
            ProcessorMonitorContainer.ComponentStopper.Remove(ComponentRegisterName);
        }

        public void ClearComponent()
        {
            // Clear messages
            VisibleConsoleMessages = new HashSet<LogEventsVM>();

            // Clear counters
            DbProcessor?.CleanData(ComponentRegisterName);

            // Clear visible counters 
            foreach (var lvlCounter in ComponentLevels)
            {
                lvlCounter.Value.Counter = 0;
            }
        }
        
        private static string TransformFilterString(string filterString)
        {
            return filterString.ToLower().Trim();
        }

        private static ComponentVM CopyComponent(ComponentVM componentToCopy)
        {
//            var newComponent = ComponentFactory.GetNewComponent(componentToCopy.ComponentType, componentToCopy.Name, componentToCopy.Path);
//
//            componentToCopy.GetType().GetProperties()
//                .Join(newComponent.GetType().GetProperties(),
//                    itemSource => itemSource.Name, itemTarget => itemTarget.Name,
//                    (propSource, propTarget) => new {PropertySource = propSource, PropertyTarget = propTarget})
//                .ToList()
//                .ForEach((item) =>
//                {
//                    // check for read-only properties
//                    if (item.PropertyTarget.SetMethod == null) return;
//
//                    item.PropertyTarget.SetValue(newComponent, item.PropertySource.GetValue(componentToCopy, null),
//                        null);
//                });
//
//            return newComponent;
            return new ComponentVM("", "", ComponentTypes.File);
        }
        
        private static void PlaySound()
        {
            SystemSounds.Hand.Play();
        }
    }
}