using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Owin.Hosting;
using LiveViewer.Utils;
using static LiveViewer.ViewModel.LogEventsVM;
using static LiveViewer.Utils.States;
using GalaSoft.MvvmLight.Command;

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

        private string componentName = Constants.Component.DefaultHttpName;
        public string ComponentName
        {
            get { return componentName; }
            set { componentName = value; NotifyPropertyChanged(); }
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
                    /* Check mandatory fields */
                    if (String.IsNullOrEmpty(HttpPath) || String.IsNullOrEmpty(HttpRoute) || String.IsNullOrEmpty(ComponentName))
                    {
                        MessageBox.Show("Component name, Http path and route are mandatory", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    if (!HttpPath.Contains("http"))
                    {
                        MessageBox.Show("Invalid Http path", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    /* Check if component already exists */
                    if (Components.Any(x => x.Name == this.ComponentName || x.Path == this.HttpPath))
                    {
                        MessageBox.Show("Component already on the Components list", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    // create new component
                    newComponent = new HttpComponentVM(this.ComponentName, HttpPath, HttpRoute)
                    {
                        RemoveComponentCommand = new RelayCommand<object>(comp =>
                        {
                            var compVM = comp as ComponentVM;
                            compVM.RemoveComponent();
                            Components.Remove(compVM);
                        })
                    };

                    // clear input data
                    //this.HttpPath = string.Empty;
                    //this.HttpRoute = string.Empty;
                    this.ComponentName = String.Empty;
                }
                else
                {
                    /* Check mandatory fields */
                    if (String.IsNullOrEmpty(FilePath) || String.IsNullOrEmpty(FileName))
                    {
                        MessageBox.Show("File path and name are mandatory", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    /* Check if component already exists */
                    if (Components.Any(x => x.Name == this.FileName || x.Path == this.FilePath))
                    {
                        MessageBox.Show("Component already on the Components list", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    /* Check if file exists */
                    if (!FileProcessor.Exists(FilePath))
                    {
                        MessageBox.Show("File doesn't exist", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    // create new component
                    newComponent = new FileComponentVM(this.FileName, this.FilePath)
                    {
                        RemoveComponentCommand = new RelayCommand<object>(comp =>
                        {
                            var compVM = comp as ComponentVM;
                            compVM.RemoveComponent();
                            Components.Remove(compVM);
                        })
                    };

                    // clear input data
                    this.FileName = string.Empty;
                    //this.FilePath = string.Empty;
                }

                #region Global
                // add new listener to list
                Components.Add(newComponent);

                // select last added component
                SelectedComponent = Components.First(x => x == newComponent);
                #endregion
            });

            /* Set cleanup command */
            CleanUpCommand = new RelayCommand(() =>
            {
                /* Clear data for each component */
                foreach (var item in Components)
                {
                    item.CleanUpCommand.Execute(!item.IsRunning);
                }
            });
        }
    }
}
