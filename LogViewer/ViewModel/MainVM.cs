using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Model;
using System.IO;
using LogViewer.Services;
using static LogViewer.Services.VisualCacheGetter;
using LogViewer.Containers;
using System.Windows.Threading;

namespace LogViewer.ViewModel
{
    public class MainVM : PropertyChangesNotifier
    {
        private string startRAMButtonImage;
        public string StartRAMButtonImage => GetCachedValue(ref startRAMButtonImage, $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}");

        private string startDiskButtonImage;
        public string StartDiskButtonImage => GetCachedValue(ref startDiskButtonImage, $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}");

        private string clearButtonImage;
        public string ClearButtonImage => GetCachedValue(ref clearButtonImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageClear}");

        private string resetButtonImage;
        public string ResetButtonImage => GetCachedValue(ref resetButtonImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageReset}");

        private string cancelButtonImage;
        public string CancelButtonImage => GetCachedValue(ref cancelButtonImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}");

        private string addImage;
        public string AddImage => GetCachedValue(ref addImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageAdd}");

        private string componentTypeLabel;
        public string ComponentTypeLabel => GetCachedValue(ref componentTypeLabel, $"{Constants.Labels.ComponentType}:");

        private string componentNameLabel;
        public string ComponentNameLabel => GetCachedValue(ref componentNameLabel, $"{Constants.Labels.ComponentName}:");

        private string pathLabel;
        public string PathLabel => GetCachedValue(ref pathLabel, $"{Constants.Labels.Path}:");

        private string startAllRAMTooltip;
        public string StartAllRAMTooltip => GetCachedValue(ref startAllRAMTooltip, Constants.Tooltips.StartAllRAM);

        private string startAllDiskTooltip;
        public string StartAllDiskTooltip => GetCachedValue(ref startAllDiskTooltip, Constants.Tooltips.StartAllDisk);

        private string cancelAllTooltip;
        public string CancelAllTooltip => GetCachedValue(ref cancelAllTooltip, Constants.Tooltips.CancelAll);

        private string clearAllTooltip;
        public string ClearAllTooltip => GetCachedValue(ref clearAllTooltip, Constants.Tooltips.ClearAll);

        private string resetAllTooltip;
        public string ResetAllTooltip => GetCachedValue(ref resetAllTooltip, Constants.Tooltips.ResetAll);

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; NotifyPropertyChanged(); }
        }

        private ComponentVM selectedComponent;
        public ComponentVM SelectedComponent
        {
            get { return selectedComponent; }
            set { selectedComponent = value; NotifyPropertyChanged(); }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; NotifyPropertyChanged(); }
        }

        public Visibility VisibleComponents
        {
            get
            {
                return Components.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public ComponentSelectorVM[] ComponentTypes => new ComponentSelectorVM[]
        {
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}", Type = Model.ComponentTypes.File },
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}", Type = Model.ComponentTypes.Http },
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageTcp}", Type = Model.ComponentTypes.Tcp },
            new ComponentSelectorVM { Icon = $"{Constants.Images.ImagePath}{Constants.Images.ImageUdp}", Type = Model.ComponentTypes.Udp }
        };

        private ComponentSelectorVM selectedComponentType;
        public ComponentSelectorVM SelectedComponentType
        {
            get { return selectedComponentType; }
            set { selectedComponentType = value; NotifyPropertyChanged(); }
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
                return new RelayCommand<object>(comp =>
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
            AddListenerCommand = new RelayCommand(() =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    if (SelectedComponentType == null)
                    {
                        MessageBox.Show(Constants.Messages.ComponentTypeMissing, Constants.Messages.ErrorTitle);
                        return;
                    }

                    // create new component
                    ComponentVM newComponent = null;
                    switch (SelectedComponentType.Type)
                    {
                        case Model.ComponentTypes.File:
                            newComponent = ComponentFactory<FileComponentVM>.GetComponent(this, Name, Path, Components, (name, path) => new FileComponentVM(name, path));
                            break;

                        case Model.ComponentTypes.Http:
                            newComponent = ComponentFactory<HttpComponentVM>.GetComponent(this, Name, Path, Components, (name, path) => new HttpComponentVM(name, path));
                            break;

                        case Model.ComponentTypes.Tcp:
                            newComponent = ComponentFactory<TcpComponentVM>.GetComponent(this, Name, Path, Components, (name, path) => new TcpComponentVM(name, path));
                            break;

                        case Model.ComponentTypes.Udp:
                            newComponent = ComponentFactory<UdpComponentVM>.GetComponent(this, Name, Path, Components, (name, path) => new UdpComponentVM(name, path));
                            break;
                    }

                    // not a valid component
                    if (newComponent == null)
                    {
                        return;
                    }

                    // set component action
                    newComponent.RemoveComponentCommand = DefaultRemoveCommand;

                    // add new listener to list
                    Components.Add(newComponent);

                    // clean selection inputs
                    Name = string.Empty;
                    Path = string.Empty;

                    // select last added component
                    SelectedComponent = Components.First(x => x == newComponent);
                }));
            });

            // Set play (ram) all command
            StartAllRAMCommand = new RelayCommand(() =>
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
            StartAllDiskCommand = new RelayCommand(() =>
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
            CancelAllCommand = new RelayCommand(() =>
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
            ClearCommand = new RelayCommand(() =>
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
            ResetCommand = new RelayCommand(() =>
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

            /* Set message collection onchanged event (DISK) */
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

            /* Add listener for visible content */
            Components.CollectionChanged += (_, e) => { NotifyPropertyChanged(nameof(VisibleComponents)); };
        }

        public void AddDroppedComponents(string[] files)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, (Action)(() =>
            {
                foreach (var file in files)
                {
                    // define new file component
                    ComponentVM comp = ComponentFactory<FileComponentVM>.GetComponent(this, new DirectoryInfo(file).Name, file, Components, (name, path) => new FileComponentVM(name, path));
                    comp.RemoveComponentCommand = DefaultRemoveCommand;

                    // add new listener to list
                    Components.Add(comp);

                    // select last added component
                    SelectedComponent = Components.First(x => x == comp);
                }
            }));
        }
    }
}
