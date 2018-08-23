using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Model;
using System.IO;

namespace LogViewer.ViewModel
{
    public class MainVM : PropertyChangesNotifier
    {
        #region Visual properties
        public string ComponentTypeLabel => $"{Constants.Labels.ComponentType}:";
        public string ComponentNameLabel => $"{Constants.Labels.ComponentName}:";
        public string PathLabel => $"{Constants.Labels.Path}:";

        public ObservableCollection<ComponentVM> Components { get; set; } = new ObservableCollection<ComponentVM>();

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
        #endregion

        #region Commands
        public ICommand AddListenerCommand { get; set; }
        public ICommand CleanUpCommand { get; set; }
        public ICommand DropCommand { get; set; }
        #endregion

        #region Images
        public string ClearButtonImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageClear}";
        public string AddImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageAdd}";
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

            // Set cleanup command 
            CleanUpCommand = new RelayCommand(() =>
            {
                // Clear data for each component 
                foreach (var item in Components)
                {
                    item.CleanUpCommand.Execute(!item.IsRunning);
                }
            });

            // Set drag and drop command
            DropCommand = new RelayCommand(() => {
                MessageBox.Show("çpç");
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
