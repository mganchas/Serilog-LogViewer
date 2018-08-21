using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LiveViewer.Configs;
using LiveViewer.Model;
using LiveViewer.Services;
using LiveViewer.Types;

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
            // Add new message queue
            MessageContainer.FileMessages.Add(ComponentRegisterName, new ObservableCollection<Entry>());

            // Set start/stop command 
            StartStopListenerCommand = new RelayCommand(() =>
            {
                IsRunning = !IsRunning;
                try
                {
                    if (asyncWorker.IsBusy)
                    {
                        asyncWorker.CancelAsync();
                        //timer.Stop();
                    }
                    else
                    {
                        // Clear previous entries 
                        CleanUpCommand.Execute(true);

                        // Set timer event
                        //timer = new System.Timers.Timer(Constants.Component.DefaultTimer);
                        //timer.Elapsed += delegate
                        //{
                        //    FilterMessages();
                        //};
                        //timer.Enabled = true;

                        // Set background worker 
                        InitializeBackWorker();
                        asyncWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    cancelSource.Cancel();
                    //timer.Stop();
                    MessageBox.Show($"Exception occurred: {ex.Message}");
                }
            });

            /* Set message collection onchanged event */
            MessageContainer.FileMessages[ComponentRegisterName].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    foreach (Entry entry in e.NewItems)
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            /* increment specific button counter */
                            ComponentLevels[entry.LevelType].Counter++;
                            ComponentLevels[Levels.LevelTypes.All].Counter++;

                            /* add item to console messages */
                            ConsoleMessages.Add(new LogEventsVM
                            {
                                RenderedMessage = entry.RenderedMessage,
                                Timestamp = entry.Timestamp,
                                LevelType = entry.LevelType
                            });
                        });
                    }

                    FilterMessages();
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
            asyncWorker.RunWorkerCompleted += delegate 
            {
                if (IsRunning) { IsRunning = false; }
                PlaySound();
            };
            asyncWorker.DoWork += (sender, e) =>
            {
                BackgroundWorker bwAsync = sender as BackgroundWorker;
                try
                {
                    cancelSource = new CancellationTokenSource();
                    var fp = new FileProcessor(Path, ComponentRegisterName);
                    fp.ReadFile(cancelSource.Token, ref asyncWorker);

                    while (!e.Cancel && !cancelSource.Token.IsCancellationRequested)
                    {
                        if (bwAsync.CancellationPending)
                        {
                            e.Cancel = true;
                            cancelSource.Cancel();
                            //timer.Stop();
                        }
                    }
                }
                catch (Exception ex)
                {
                    asyncWorker.CancelAsync();
                    cancelSource.Cancel();
                    //timer.Stop();
                    MessageBox.Show(ex.Message, "Error");
                }
            };
        }

        public static bool IsValidComponent(string name, string path, in ObservableCollection<ComponentVM> components)
        {
            // Check mandatory fields 
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(name))
            {
                MessageBox.Show("File path and name are mandatory", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            // Check if component already exists 
            if (components.Any(x => x.Name == name || x.Path == path))
            {
                MessageBox.Show("Component already on the Components list", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            // Check if file exists 
            if (!FileProcessor.Exists(path))
            {
                MessageBox.Show("File doesn't exist", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.FileMessages.Remove(ComponentRegisterName);
        }

        public override void ClearComponent()
        {
            MessageContainer.FileMessages[ComponentRegisterName].Clear();
        }
    }
}
