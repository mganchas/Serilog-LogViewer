using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Utils;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.ViewModel
{
    public class FileComponentVM : ComponentVM
    {
        public override string ComponentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}";
        public override string SearchImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public override string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";

        private CancellationTokenSource cancelSource;

        public FileComponentVM(string name, string path) : base(name, path)
        {
            /* Add new message queue */
            MessageContainer.FileMessages.Add(Name, new System.Collections.ObjectModel.ObservableCollection<LogEvent>());

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
                        /* Clear previous entries */
                        CleanUpCommand.Execute(true);

                        /* Set background worker */
                        InitializeBackWorker();
                        asyncWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    cancelSource.Cancel();
                    asyncWorker.CancelAsync();
                    MessageBox.Show($"Exception occurred: {ex.Message}");
                }
            });

            /* Set message collection onchanged event */
            MessageContainer.FileMessages[this.Name].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    foreach (LogEvent msg in e.NewItems)
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
                    cancelSource.Cancel();
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
                    cancelSource = new CancellationTokenSource();
                    var fp = new FileProcessor(Path, Name);
                    fp.ReadFile(cancelSource.Token, ref asyncWorker);

                    while (!e.Cancel && !cancelSource.Token.IsCancellationRequested)
                    {
                        if (bwAsync.CancellationPending)
                        {
                            e.Cancel = true;
                            cancelSource.Cancel();
                        }
                    }
                }
                catch (Exception ex)
                {
                    cancelSource.Cancel();
                    asyncWorker.CancelAsync();
                    MessageBox.Show(ex.Message, "Error");
                }
            };
        }

        public override void RemoveComponent()
        {
            MessageContainer.FileMessages.Remove(this.Name);
        }

        public override void ClearComponent()
        {
            MessageContainer.FileMessages[this.Name].Clear();
        }
    }
}
