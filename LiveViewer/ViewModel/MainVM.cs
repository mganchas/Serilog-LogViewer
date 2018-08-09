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
        public ObservableCollection<ComponentVM> Components { get; set; } = new ObservableCollection<ComponentVM>();

        private string componentName = Constants.DefaultName;
        public string ComponentName
        {
            get { return componentName; }
            set { componentName = value; NotifyPropertyChanged(); }
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

        private ComponentVM selectedComponent;
        public ComponentVM SelectedComponent
        {
            get { return selectedComponent; }
            set { selectedComponent = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand AddListenerCommand { get; set; }
        public ICommand CleanUpCommand { get; set; }
        public ICommand RemoveComponentCommand { get; set; }
        public ICommand EditComponentCommand { get; set; }
        #endregion

        #region Images
        private static readonly string SearchImage = $"{Constants.ImagePath}{Constants.ImageSearch}";
        private static readonly string CancelImage = $"{Constants.ImagePath}{Constants.ImageCancel}";

        public string ClearButtonImage => $"{Constants.ImagePath}{Constants.ImageClear}";
        public string AddImage => $"{Constants.ImagePath}{Constants.ImageAdd}";
        #endregion

        public MainVM()
        {
            /* Set remove command for components list */
            RemoveComponentCommand = new RelayCommand(() =>
            {
                /* Stop listening for component (if it's running) */
                if (SelectedComponent.IsRunning) { SelectedComponent.StartStopListenerCommand.Execute(null); }

                /* Remove from components list */
                Components.Remove(SelectedComponent);
            });

            AddListenerCommand = new RelayCommand(() =>
            {
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
                if (Components.Any(x => x.Name == this.ComponentName || x.HttpPath == this.HttpPath || x.HttpRoute == this.HttpRoute))
                {
                    MessageBox.Show("Component already on the Components list", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    // add new listener to list
                    var newComponent = new ComponentVM
                    {
                        HttpPath = this.HttpPath,
                        HttpRoute = this.HttpRoute,
                        Name = this.ComponentName,
                        RemoveComponentCommand = new RelayCommand<object>(comp =>
                        {
                            Components.Remove(comp as ComponentVM);
                        })
                    };
                    Components.Add(newComponent);

                    // select last added component
                    SelectedComponent = Components.First(x => x == newComponent);

                    // clear input data
                    this.HttpPath = string.Empty;
                    this.HttpRoute = string.Empty;
                    this.ComponentName = String.Empty;
                }
            });

            /* Set cleanup command */
            CleanUpCommand = new RelayCommand(() =>
            {
                /* Clear data for each component */
                foreach (var item in Components)
                {
                    item.CleanUpCommand.Execute(null);
                }
            });

        }
    }
}
