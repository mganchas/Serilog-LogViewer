using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using LogViewer.Configs;
using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services;
using static LogViewer.Services.VisualCacheGetter;

namespace LogViewer.ViewModel
{
    public class TcpComponentVM : ComponentVM
    {
        private string componentImage;
        public override string ComponentImage => GetCachedValue(ref componentImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageTcp}");

        private string TcpFullName => Path.EndsWith("/") ? $"{Path.Substring(0, Path.Length - 1)}" : Path;

        public TcpComponentVM(string name, string path) : base(name, path, ComponentTypes.Tcp)
        {
            // Set message collection onchanged event (DISK) 
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].CollectionChanged += (sender, e) =>
            {
                CollectionChangedDISK(e, sender as ObservableCounterDictionary<Levels.LevelTypes>);
            };

            // Add new message queue
            MessageContainer.RAM.TcpMessages.Add(ComponentRegisterName, new Lazy<ObservableSet<Entry>>());

            // Set message collection onchanged event 
            MessageContainer.RAM.TcpMessages[ComponentRegisterName].Value.CollectionChanged += (sender, e) =>
            {
                CollectionChangedRAM(e, sender as ObservableSet<Entry>);
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
                    if (bwAsync.CancellationPending) { return; }

                    var tcpP = new TcpProcessor();
                    tcpP.ReadData(Path, ComponentRegisterName, ref asyncWorker, StoreType);

                    while (!e.Cancel)
                    {
                        if (bwAsync.CancellationPending)
                        {
                            e.Cancel = true;
                            StopListener();
                        }
                    }

                }
                catch (Exception ex)
                {
                    LoggerContainer.LogEntries.Add(new LogVM
                    {
                        Timestamp = DateTime.Now,
                        Message = ex.InnerException?.Message ?? ex.Message
                    });
                    StopListener();
                }
            };
        }

        public override bool IsValidComponent(in Span<ComponentVM> components)
        {
            // Check mandatory fields 
            if (String.IsNullOrEmpty(Path) || String.IsNullOrEmpty(Name))
            {
                MessageBox.Show(Constants.Messages.MandatoryFieldsMissingComponent, Constants.Messages.AlertTitle);
                return false;
            }
            if (Path.ToLower().Contains("tcp"))
            {
                MessageBox.Show(Constants.Messages.InvalidUrlComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if component already exists 
            foreach (var comp in components)
            {
                if (comp.Name == Name || comp.Path == Path)
                {
                    MessageBox.Show(Constants.Messages.DuplicateComponent, Constants.Messages.AlertTitle);
                    return false;
                }
            }
            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.RAM.TcpMessages.Remove(ComponentRegisterName);
            MessageContainer.Disk.ComponentCounters.Remove(ComponentRegisterName);
            ProcessorMonitorContainer.ComponentStopper.Remove(ComponentRegisterName);
        }

        public override void ClearComponent()
        {
            MessageContainer.RAM.TcpMessages[ComponentRegisterName].Value.Clear();
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].Clear();
        }
    }
}
