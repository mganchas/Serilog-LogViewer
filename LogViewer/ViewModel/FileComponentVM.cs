using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using LogViewer.Configs;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;

namespace LogViewer.ViewModel
{
    public class FileComponentVM : ComponentVM, ICustomComponent
    {
        public override string ComponentImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageFile}";
        public override string SearchImage => $"{Constants.Images.ImagePath}{Constants.Images.ImagePlay}";
        public override string CancelImage => $"{Constants.Images.ImagePath}{Constants.Images.ImageCancel}";
        private CancellationTokenSource cancelSource;

        public FileComponentVM(string name, string path) : base(name, path)
        {
            // Add new message queue
            MessageContainer.FileMessages.Add(ComponentRegisterName, new ObservableSet<Entry>());
            
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
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
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
                    fp.ReadData(ref cancelSource, ref asyncWorker);

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
                    asyncWorker.CancelAsync();
                    cancelSource.Cancel();
                    MessageBox.Show(ex.Message, Constants.Messages.ErrorTitle);
                }
            };
        }

        public static bool IsValidComponent(string name, string path, in ObservableCollection<ComponentVM> components)
        {
            // Check mandatory fields 
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(name))
            {
                MessageBox.Show(Constants.Messages.MandatoryFieldsMissingComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if component already exists 
            if (components.Any(x => x.Name == name || x.Path == path))
            {
                MessageBox.Show(Constants.Messages.DuplicateComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if file exists 
            if (!FileProcessor.Exists(path))
            {
                MessageBox.Show(Constants.Messages.FileNotFoundComponent, Constants.Messages.AlertTitle);
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
