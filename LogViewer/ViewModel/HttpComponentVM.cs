﻿using System;
using System.ComponentModel;
using System.Windows;
using LogViewer.Configs;
using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services;

namespace LogViewer.ViewModel
{
    public class HttpComponentVM : ComponentVM
    {
        private static string componentImage;
        public override string ComponentImage {
            get
            {
                if (componentImage == null)
                {
                    componentImage = $"{Constants.Images.ImagePath}{Constants.Images.ImageHttp}";
                }
                return componentImage;
            }
        }

        private string PathFixer => !Path.EndsWith("/") ? $"{Path}/" : Path;
        private string HttpFullName => $"http://{PathFixer}";

        public HttpComponentVM(string name, string path) : base(name, path, ComponentTypes.Http)
        {
            // Set message collection onchanged event (DISK) 
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].CollectionChanged += (sender, e) =>
            {
                CollectionChangedDISK(e, sender as ObservableCounterDictionary<Levels.LevelTypes>);
            };

            // Add new message queue
            MessageContainer.RAM.HttpMessages.Add(ComponentRegisterName, new Lazy<ObservableSet<Entry>>());

            // Set message collection onchanged event 
            MessageContainer.RAM.HttpMessages[ComponentRegisterName].Value.CollectionChanged += (sender, e) =>
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
                    if (bwAsync != null && bwAsync.CancellationPending) { return; }

                    var httpP = new HttpProcessor();
                    httpP.ReadData(HttpFullName, ComponentRegisterName, ref asyncWorker, StoreType);

                    while (!e.Cancel)
                    {
                        if (bwAsync == null || !bwAsync.CancellationPending) continue;
                        e.Cancel = true;
                        StopListener();
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
            if (Path.ToLower().Contains("http"))
            {
                MessageBox.Show(Constants.Messages.InvalidUrlComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if component already exists 
            foreach (var comp in components)
            {
                if (comp.Name != Name && comp.Path != Path) continue;
                MessageBox.Show(Constants.Messages.DuplicateComponent, Constants.Messages.AlertTitle);
                return false;
            }

            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.RAM.HttpMessages.Remove(ComponentRegisterName);
            MessageContainer.Disk.ComponentCounters.Remove(ComponentRegisterName);
            ProcessorMonitorContainer.ComponentStopper.Remove(ComponentRegisterName);
        }

        public override void ClearComponent()
        {
            MessageContainer.RAM.HttpMessages[ComponentRegisterName].Value.Clear();
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].Clear();
        }
    }
}
