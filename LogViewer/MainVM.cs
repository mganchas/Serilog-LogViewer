using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LogViewer.Components;
using LogViewer.Components.Processors;
using LogViewer.Configs;
using LogViewer.Logging;
using LogViewer.Structures.Containers;
using LogViewer.ViewModel.Helpers;

namespace LogViewer
{
    public class MainVM : PropertyChangesNotifier
    {
        public static string StartRAMButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}";
        public static string StartDiskButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public static string ClearButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageClear}";
        public static string ResetButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageReset}";
        public static string CancelButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        public static string AddImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageAdd}";
        public static string ComponentTypeLabel => $"{Constants.Labels.ComponentType}:";
        public static string ComponentNameLabel => $"{Constants.Labels.ComponentName}:";
        public static string PathLabel => $"{Constants.Labels.Path}:";
        public static string StartAllRAMTooltip => Constants.Tooltips.StartAllRAM;
        public static string StartAllDiskTooltip => Constants.Tooltips.StartAllDisk;
        public static string CancelAllTooltip => Constants.Tooltips.CancelAll;
        public static string ClearAllTooltip => Constants.Tooltips.ClearAll;
        public static string ResetAllTooltip => Constants.Tooltips.ResetAll;

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; NotifyPropertyChanged(); }
        }

        private ComponentVM _selectedComponent;
        public ComponentVM SelectedComponent
        {
            get { return _selectedComponent; }
            set { _selectedComponent = value; NotifyPropertyChanged(); }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; NotifyPropertyChanged(); }
        }

        public Visibility VisibleComponents => Components == null || Components.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        public ComponentSelectorVM[] ComponentTypes { get; } = 
        {
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}", Type = LogViewer.Components.ComponentTypes.File },
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}", Type = LogViewer.Components.ComponentTypes.Http },
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageTcp}", Type = LogViewer.Components.ComponentTypes.Tcp },
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageUdp}", Type = LogViewer.Components.ComponentTypes.Udp }
        };

        private ComponentSelectorVM _selectedComponentType;
        public ComponentSelectorVM SelectedComponentType
        {
            get { return _selectedComponentType; }
            set { _selectedComponentType = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<ComponentVM> Components { get; set; } = new ObservableCollection<ComponentVM>();
        public ObservableCollection<LogVM> LogEntries { get; set; } = new ObservableCollection<LogVM>();

        #region Commands
        public ICommand AddListenerCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand StartAllRAMCommand { get; set; }
        public ICommand StartAllDiskCommand { get; set; }
        public ICommand CancelAllCommand { get; set; }
        private ICommand DefaultRemoveCommand
        {
            get
            {
                return new CommandHandler(comp =>
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        var compVM = comp as ComponentVM;

                        compVM.RemoveComponent();
                        Components.Remove(compVM);

                        SelectedComponent = Components.Count > 0 ? Components[0] : null;

                        GC.Collect();
                    }));
                });
            }
        }
        #endregion

        public MainVM()
        {
            AddListenerCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    if (SelectedComponentType == null)
                    {
                        MessageBox.Show(Constants.Messages.ComponentTypeMissing, Constants.Messages.ErrorTitle);
                        return; 
                    }

//                    // create new component
//                    var newComponent = ComponentFactory.GetNewComponent(SelectedComponentType.Type);
//                    
//                    // not a valid component
//                    if (!newComponent.IsValidComponent(new Span<ComponentVM>(Components.ToArray()))) { return; }
//
//                    // register component behaviour
//                    newComponent.RegisterComponent();
//                    
//                    // set component action
//                    newComponent.RemoveComponentCommand = DefaultRemoveCommand;
//
//                    // add new listener to list
//                    Components.Add(newComponent);
//
//                    // clean selection inputs
//                    Name = string.Empty;
//                    Path = string.Empty;
//
//                    // select last added component
//                    SelectedComponent = Components.First(x => x == newComponent);
                }));
            });

            // Set play (ram) all command
            StartAllRAMCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    // if not running, start each component 
                    foreach (var item in Components.Where(x => !x.IsRunning))
                    {
                        item.StartRAMListenerCommand.Execute(null);
                    }
                }));
            });

            // Set play (disk) all command
            StartAllDiskCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    // if not running, start each component 
                    foreach (var item in Components.Where(x => !x.IsRunning))
                    {
                        item.StartDiskListenerCommand.Execute(null);
                    }
                }));
            });

            // set cancel all command
            CancelAllCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, (Action)(() =>
                {
                    // if not running, start each component 
                    foreach (var item in Components.Where(x => x.IsRunning))
                    {
                        item.StopListenerCommand.Execute(null);
                    }
                }));
            });

            // Set clear command 
            ClearCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    // Clear data for each component 
                    foreach (var item in Components)
                    {
                        item.CleanUpCommand.Execute(!item.IsRunning);
                    }
                    GC.Collect();
                }));
            });

            // Set reset command 
            ResetCommand = new CommandHandler(_ =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    foreach (var comp in Components)
                    {
                        comp.RemoveComponent();
                    }
                    Components.Clear();
                    GC.Collect();
                }));
            });

            // Set message collection onchanged event (DISK) 
            LoggerContainer.LogEntries.CollectionChanged += (_, e) =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                {
                    foreach (LogVM exception in e.NewItems)
                    {
                        LogEntries.Add(exception);
                    }
                }));
            };

            // Add listener for visible content 
            Components.CollectionChanged += (_, e) => { NotifyPropertyChanged(nameof(VisibleComponents)); };
        }

        public void AddDroppedComponents(string[] files)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, (Action)(() =>
            {
                foreach (var file in files)
                {
//                    // define new file component
//                    ComponentVM newComponent = ComponentFactory.GetNewComponent(LogViewer.Components.ComponentTypes.File, new DirectoryInfo(file).Name, file);
//
//                    // not a valid component
//                    if (!newComponent.IsValidComponent(new Span<ComponentVM>(Components.ToArray()))) { return; }
//
//                    // register component behaviour
//                    newComponent.RegisterComponent();
//
//                    // set component action
//                    newComponent.RemoveComponentCommand = DefaultRemoveCommand;
//
//                    // add new listener to list
//                    Components.Add(newComponent);
//
//                    // select last added component
//                    SelectedComponent = Components.First(x => x == newComponent);
                }
            }));
        }
    }
}
