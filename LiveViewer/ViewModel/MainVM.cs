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

namespace LiveViewer.ViewModel
{
    public class MainVM : BaseVM
    {
        private string componentName = Constants.DefaultName;
        public string ComponentName
        {
            get { return componentName; }
            set { componentName = value; NotifyPropertyChanged(); }
        }

        public string HttpPath { get; set; } = Constants.DefaultHttpPath;
        public string HttpRoute { get; set; } = Constants.DefaultHttpRoute;
        public ComponentVM SelectedComponent { get; set; }

        #region Collections
        public ObservableCollection<ComponentVM> Components { get; set; } = new ObservableCollection<ComponentVM>();
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
            /* add dummy components */
            Components.Add(new ComponentVM { Name = "Batatas", HttpPath = Constants.DefaultHttpPath, IsRunning = true, HttpRoute = Constants.DefaultHttpRoute });
            Components.Add(new ComponentVM { Name = "Couves", HttpPath = Constants.DefaultHttpPath, IsRunning = false, HttpRoute = Constants.DefaultHttpRoute });
            Components.Add(new ComponentVM { Name = "Cenouras", HttpPath = Constants.DefaultHttpPath, IsRunning = true, HttpRoute = Constants.DefaultHttpRoute });
            Components.Add(new ComponentVM { Name = "Alho Francês", HttpPath = Constants.DefaultHttpPath, IsRunning = true, HttpRoute = Constants.DefaultHttpRoute });
            Components.Add(new ComponentVM { Name = "Couve-Flor", HttpPath = Constants.DefaultHttpPath, IsRunning = true, HttpRoute = Constants.DefaultHttpRoute });

            /* Set remove command for components list */
            RemoveComponentCommand = new GenericCommand(() =>
            {
                /* Stop listening for component (if it's running) */
                if (SelectedComponent.IsRunning) { SelectedComponent.StartStopListenerCommand.Execute(null); }

                /* Remove from components list */
                Components.Remove(SelectedComponent);
            });

            AddListenerCommand = new GenericCommand(() =>
            {
                if (String.IsNullOrEmpty(HttpPath) || String.IsNullOrEmpty(HttpRoute) || String.IsNullOrEmpty(ComponentName))
                {
                    MessageBox.Show("Component name, Http path and route are mandatory", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                /* Check if component already exists */
                if (Components.Any(x => x.Name == this.ComponentName))
                {
                    MessageBox.Show("Component already on the Components list", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    Components.Add(new ComponentVM
                    {
                        HttpPath = this.HttpPath,
                        HttpRoute = this.HttpRoute,
                        Name = this.ComponentName
                    });
                    this.ComponentName = String.Empty;
                }
            });

            /* Set cleanup command */
            CleanUpCommand = new GenericCommand(() =>
            {
                /* Clear data for each component */
                foreach (var item in Components)
                {
                    item.CleanUpCommand.Execute(null);
                }
            }, (s) => { return true; });
        }
    }
}
