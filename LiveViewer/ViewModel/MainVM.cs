using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Services;
using LiveViewer.Configs;

namespace LiveViewer.ViewModel
{
    public class MainVM : BaseVM
    {
        #region Visual properties
        public string ComponentNameLabel => $"{Constants.Labels.ComponentName}:";
        public string HttpPathLabel => $"{Constants.Labels.HttpPath}:";
        public string HttpRouteLabel => $"{Constants.Labels.HttpRoute}:";
        public string FileNameLabel => $"{Constants.Labels.FileName}:";
        public string FilePathLabel => $"{Constants.Labels.FilePath}:";

        public ObservableCollection<ComponentVM> Components { get; set; } = new ObservableCollection<ComponentVM>();

        private string httpName = Constants.Component.DefaultHttpName;
        public string HttpName
        {
            get { return httpName; }
            set { httpName = value; NotifyPropertyChanged(); }
        }

        private string httpPath = Constants.Component.DefaultHttpPath;
        public string HttpPath
        {
            get { return httpPath; }
            set { httpPath = value; NotifyPropertyChanged(); }
        }

        private string httpRoute = Constants.Component.DefaultHttpRoute;
        public string HttpRoute
        {
            get { return httpRoute; }
            set { httpRoute = value; NotifyPropertyChanged(); }
        }

        private string fileName = Constants.Component.DefaultFileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; NotifyPropertyChanged(); }
        }

        private string filePath = Constants.Component.DefaultFilePath;
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; NotifyPropertyChanged(); }
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

        private bool httpSelector = true;
        public bool HttpSelector
        {
            get { return httpSelector; }
            set { httpSelector = value; NotifyPropertyChanged(); }
        }

        private bool fileSelector;
        public bool FileSelector
        {
            get { return fileSelector; }
            set { fileSelector = value; NotifyPropertyChanged(); }
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
                ComponentVM newComponent;
                if (httpSelector)
                {
                    // check if component is valid
                    if (!HttpComponentVM.IsValidComponent(HttpName, HttpPath, HttpRoute, Components)) { return; }

                    // create new component
                    newComponent = new HttpComponentVM(this.HttpName, HttpPath, HttpRoute)
                    {
                        RemoveComponentCommand = new RelayCommand<object>(comp =>
                        {
                            var compVM = comp as ComponentVM;
                            Components.Remove(compVM);
                        })
                    };
                }
                else
                {
                    // check if component is valid
                    if (!FileComponentVM.IsValidComponent(FileName, FilePath, Components)) { return; } 

                    // create new component
                    newComponent = new FileComponentVM(this.FileName, this.FilePath)
                    {
                        RemoveComponentCommand = new RelayCommand<object>(comp =>
                        {
                            var compVM = comp as ComponentVM;
                            Components.Remove(compVM);
                        })
                    };
                }

                #region Global
                // add new listener to list
                Components.Add(newComponent);

                // select last added component
                SelectedComponent = Components.First(x => x == newComponent);
                #endregion
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
    }
}
