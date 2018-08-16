using System;
using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Utils;
using Microsoft.Owin.Hosting;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public class HttpComponentVM : ComponentVM
    {
        public override string ComponentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}";
        public override string SearchImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public override string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        private string TransfPath => !Path.EndsWith("/") ? $"{Path}/" : Path;
        private string TransfFullHttp => $"{TransfPath}{HttpRoute}";

        private string httpRoute = Constants.Component.DefaultHttpRoute;
        public string HttpRoute
        {
            get { return httpRoute; }
            set { httpRoute = value; NotifyPropertyChanged(); }
        }

        public HttpComponentVM(string name, string path, string httpRoute) : base(name, path)
        {
            this.HttpRoute = httpRoute;

            /* Add new message queue */
            MessageContainer.HttpMessages.Add(TransfFullHttp, new System.Collections.ObjectModel.ObservableCollection<LogEvent>());

            /* Set start/stop command */
            StartStopListenerCommand = new RelayCommand(() =>
            {
                IsRunning = !IsRunning;
                try
                {
                    if (asyncWorker.IsBusy)
                    {
                        asyncWorker.CancelAsync();
                    }
                    else
                    {
                        /* Set background worker */
                        InitializeBackWorker();
                        asyncWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show($"Exception occurred: {ex.Message}");
                }
            });

            /* Set message collection onchanged event */
            MessageContainer.HttpMessages[TransfFullHttp].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    foreach (LogEvent msg in e?.NewItems)
                    {
                        msg.LevelType = Levels.GetLevelTypeFromString(msg.Level);
                        msg.LevelColor = Levels.GetLevelColor(msg.LevelType);

                        App.Current.Dispatcher.Invoke(delegate
                        {
                                /* increment specific button counter */
                            ComponentLevels[msg.LevelType].Counter++;
                            ComponentLevels[Levels.LevelTypes.All].Counter++;

                                /* add item to console messages */
                            ConsoleMessages[msg.LevelType].Add(msg);
                        });
                        //App.Current.Dispatcher.Invoke(new Action(delegate { }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message);
                }
            };
        }

        protected override void InitializeBackWorker()
        {
            asyncWorker.WorkerReportsProgress = true;
            asyncWorker.WorkerSupportsCancellation = true;
            asyncWorker.RunWorkerCompleted += delegate { if (IsRunning) IsRunning = false; };
            asyncWorker.DoWork += (sender, e) =>
            {
                BackgroundWorker bwAsync = sender as BackgroundWorker;
                try
                {
                    using (WebApp.Start<Startup>(TransfFullHttp))
                    {
                        while (!e.Cancel)
                        {
                            if (bwAsync.CancellationPending)
                            {
                                e.Cancel = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, "Error");
                }
            };
        }

        public override void RemoveComponent()
        {
            MessageContainer.HttpMessages.Remove(this.TransfFullHttp);
        }

        public override void ClearComponent()
        {
            MessageContainer.HttpMessages[this.TransfFullHttp].Clear();
        }
    }
}
