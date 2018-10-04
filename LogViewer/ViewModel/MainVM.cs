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
using Microsoft.Extensions.Logging;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace LogViewer.ViewModel
{
    public class MainVM : PropertyChangesNotifier
    {
        #region Visual properties
        public string StartRAMButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlayBlue}";
        public string StartDiskButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public string ClearButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageClear}";
        public string ResetButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageReset}";
        
        public string CancelButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        public string AddImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageAdd}";
        public string ComponentTypeLabel => $"{Constants.Labels.ComponentType}:";
        public string ComponentNameLabel => $"{Constants.Labels.ComponentName}:";
        public string PathLabel => $"{Constants.Labels.Path}:";
        public string StartAllRAMTooltip => Constants.Tooltips.StartAllRAM;
        public string StartAllDiskTooltip => Constants.Tooltips.StartAllDisk;
        public string CancelAllTooltip => Constants.Tooltips.CancelAll;
        public string ClearAllTooltip => Constants.Tooltips.ClearAll;
        public string ResetAllTooltip => Constants.Tooltips.ResetAll;

        public ObservableCollection<ComponentVM> Components { get; set; } = new ObservableCollection<ComponentVM>();

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                LoggerService.Logger.LogDebug($"{nameof(Name)}: {Name}");
                NotifyPropertyChanged();
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                LoggerService.Logger.LogDebug($"{nameof(Path)}: {Path}");
                NotifyPropertyChanged();
            }
        }

        private ComponentVM selectedComponent;
        public ComponentVM SelectedComponent
        {
            get { return selectedComponent; }
            set
            {
                selectedComponent = value;
                //LoggerService.Logger.LogDebug($"{nameof(SelectedComponent)}: {new JavaScriptSerializer().Serialize(SelectedComponent)}");
                NotifyPropertyChanged();
            }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                LoggerService.Logger.LogDebug($"{nameof(SelectedIndex)}: {SelectedIndex}");
                NotifyPropertyChanged();
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
            set
            {
                selectedComponentType = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand AddListenerCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand StartAllRAMCommand { get; set; }
        public ICommand StartAllDiskCommand { get; set; }
        public ICommand CancelAllCommand { get; set; }
        #endregion

        public MainVM()
        {
            AddListenerCommand = new RelayCommand(() =>
            {
                if (SelectedComponentType == null)
                {
                    MessageBox.Show(Constants.Messages.ComponentTypeMissing, Constants.Messages.ErrorTitle);
                    return;
                }

                // create new component
                ComponentVM newComponent;
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

                    default:
                        throw new ArgumentException(Constants.Messages.InvalidComponentException);
                }

                // not a valid component
                if (newComponent == null) { return; }

                // set component details
                SetNewComponent(ref newComponent);

                // add new listener to list
                Components.Add(newComponent);

                // clean selection inputs
                Name = string.Empty;
                Path = string.Empty;

                // select last added component
                SelectedComponent = Components.First(x => x == newComponent);
            });

            // Set play (ram) all command
            StartAllRAMCommand = new RelayCommand(() =>
            {
                // if not running, start each component 
                foreach (var item in Components.Where(x => !x.IsRunning))
                {
                    item.StartRAMListenerCommand.Execute(null);
                }
            });

            // Set play (disk) all command
            StartAllDiskCommand = new RelayCommand(() =>
            {
                // if not running, start each component 
                foreach (var item in Components.Where(x => !x.IsRunning))
                {
                    item.StartDiskListenerCommand.Execute(null);
                }
            });

            // set cancel all command
            CancelAllCommand = new RelayCommand(() =>
            {
                // if not running, start each component 
                foreach (var item in Components.Where(x => x.IsRunning))
                {
                    item.StopListenerCommand.Execute(null);
                }
            });

            // Set clear command 
            ClearCommand = new RelayCommand(() =>
            {
                // Clear data for each component 
                foreach (var item in Components)
                {
                    item.CleanUpCommand.Execute(!item.IsRunning);
                }
            });

            // Set reset command 
            ResetCommand = new RelayCommand(() =>
            {
                foreach (var comp in Components) {
                    comp.RemoveComponent();
                }
                Components.Clear();
            });
        }

        /// <summary>
        /// Use to set generic properties for a new component (that can't be done within its class)
        /// </summary>
        /// <param name="component"></param>
        private void SetNewComponent(ref ComponentVM newComponent)
        {
            // command for when component is removed
            newComponent.RemoveComponentCommand = new RelayCommand<object>(comp =>
            {
                var compVM = comp as ComponentVM;
                compVM.RemoveComponent();
                Components.Remove(compVM);
                GC.Collect();
            });

            // add other actions (if applicable)...

        }

        public void AddDroppedComponents(string[] files)
        {
            foreach (var file in files)
            {
                // define new file component
                ComponentVM comp = ComponentFactory<FileComponentVM>.GetComponent(this, new DirectoryInfo(file).Name, file, Components, (name, path) => new FileComponentVM(name, path));
                SetNewComponent(ref comp);

                // add new listener to list
                Components.Add(comp);
            }
        }
    }
}
