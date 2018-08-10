﻿using System;
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

        public HttpComponentVM(string name) : base(ComponentTypes.Http, name)
        {
            /* Add new message queue */
            MessageContainer.HttpMessages.Add(Name, new System.Collections.ObjectModel.ObservableCollection<LogEvent>());

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
                    MessageBox.Show($"Exception occurred: {ex.Message}");
                    asyncWorker.CancelAsync();
                }
            });

            /* Set message collection onchanged event */
            MessageContainer.HttpMessages[this.Name].CollectionChanged += (sender, e) =>
            {
                if (!IsRunning) { return; }

                if (e.NewItems != null)
                {
                    try
                    {
                        foreach (LogEvent msg in e.NewItems)
                        {
                            msg.LevelType = Levels.GetLevelTypeFromString(msg.Level);
                            msg.LevelColor = Levels.GetLevelColor(msg.LevelType);

                            App.Current.Dispatcher.Invoke(delegate
                            {
                                /* increment specific button counter */
                                switch (msg.LevelType)
                                {
                                    case Levels.LevelTypes.Verbose:
                                        VerboseLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Debug:
                                        DebugLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Information:
                                        InformationLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Warning:
                                        WarningLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Error:
                                        ErrorLevel.Counter++;
                                        break;
                                    case Levels.LevelTypes.Fatal:
                                        FatalLevel.Counter++;
                                        break;
                                    default:
                                        break;
                                }

                                /* increment all category */
                                AllLevel.Counter++;

                                /* add item to current level */
                                if (((CurrentLevel == Levels.LevelTypes.All) || (CurrentLevel == msg.LevelType)) &&
                                    (!IsFiltered || (IsFiltered && msg.RenderedMessage.ToLower().Contains(FilterText.ToLower()))))
                                {
                                    VisibleConsoleMessages.Add(msg);
                                }

                                /* add item to console messages */
                                ConsoleMessages.Add(msg);
                            });
                            //App.Current.Dispatcher.Invoke(new Action(delegate { }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
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
                    using (WebApp.Start<Startup>($"{transfPath}{transfHttpRoute}"))
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
                    MessageBox.Show(ex.Message, "Error");
                }
            };
        }
    }
}