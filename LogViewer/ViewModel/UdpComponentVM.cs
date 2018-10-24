﻿using LogViewer.Configs;
using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services;
using LogViewer.ViewModel.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using static LogViewer.Services.VisualCacheGetter;

namespace LogViewer.ViewModel
{
    public class UdpComponentVM : ComponentVM, ICustomComponent
    {
        private string componentImage;
        public override string ComponentImage => GetCachedValue(ref componentImage, $"{Constants.Images.ImagePath}{Constants.Images.ImageUdp}");

        private string UdpFullName => Path.EndsWith("/") ? $"{Path.Substring(0, Path.Length - 1)}" : Path;

        public UdpComponentVM(string name, string path) : base(name, path, ComponentTypes.Udp)
        {
            /* Set message collection onchanged event (DISK) */
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    /* increment button counters */
                    foreach (var counter in (sender as ObservableCounterDictionary<Levels.LevelTypes>).GetItemSet())
                    {
                        ComponentLevels[counter.Key].Counter = counter.Value;

                    }
                    FilterMessages();
                }
                catch (Exception ex)
                {
                    LoggerContainer.LogEntries.Add(new LogVM
                    {
                        Timestamp = DateTime.Now,
                        Message = ex.InnerException?.Message ?? ex.Message
                    });
                }
            };

            // Add new message queue
            MessageContainer.RAM.UdpMessages.Add(ComponentRegisterName, new Lazy<ObservableSet<Entry>>());

            /* Set message collection onchanged event */
            MessageContainer.RAM.UdpMessages[ComponentRegisterName].Value.CollectionChanged += (sender, e) =>
            {
                if (!IsRunning || e.NewItems == null) { return; }

                try
                {
                    foreach (Entry entry in e.NewItems)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                        {
                            /* increment specific button counter */
                            ComponentLevels[(Levels.LevelTypes)entry.LevelType].Counter++;
                            ComponentLevels[Levels.LevelTypes.All].Counter++;

                            /* add item to console messages */
                            ConsoleMessages.Add(new LogEventsVM
                            {
                                RenderedMessage = entry.RenderedMessage,
                                Timestamp = entry.Timestamp,
                                LevelType = (Levels.LevelTypes)entry.LevelType
                            });
                        }));
                    }

                    FilterMessages();
                }
                catch (Exception ex)
                {
                    LoggerContainer.LogEntries.Add(new LogVM
                    {
                        Timestamp = DateTime.Now,
                        Message = ex.InnerException?.Message ?? ex.Message
                    });
                }
            };
        }

        protected override void InitializeBackWorker()
        {
            //asyncWorker.WorkerReportsProgress = true;
            asyncWorker.WorkerSupportsCancellation = true;
            asyncWorker.RunWorkerCompleted += delegate { if (IsRunning) IsRunning = false; };
            asyncWorker.DoWork += (sender, e) =>
            {
                BackgroundWorker bwAsync = sender as BackgroundWorker;
                try
                {
                    if (bwAsync.CancellationPending) { return; }

                    var udpP = new UdpProcessor();
                    udpP.ReadData(Path, ComponentRegisterName, ref asyncWorker, StoreType);

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
                    e.Cancel = true;
                    StopListener();
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
            if (path.ToLower().Contains("udp"))
            {
                MessageBox.Show(Constants.Messages.InvalidUrlComponent, Constants.Messages.AlertTitle);
                return false;
            }

            // Check if component already exists 
            if (components.Any(x => x.Name == name || x.Path == path))
            {
                MessageBox.Show(Constants.Messages.DuplicateComponent, Constants.Messages.AlertTitle);
                return false;
            }

            return true;
        }

        public override void RemoveComponent()
        {
            MessageContainer.RAM.UdpMessages.Remove(ComponentRegisterName);
            MessageContainer.Disk.ComponentCounters.Remove(ComponentRegisterName);
            ProcessorMonitorContainer.ComponentStopper.Remove(ComponentRegisterName);
        }

        public override void ClearComponent()
        {
            MessageContainer.RAM.UdpMessages[ComponentRegisterName].Value.Clear();
            MessageContainer.Disk.ComponentCounters[ComponentRegisterName].Clear();
        }
    }
}