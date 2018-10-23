using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services.Abstractions;
using LogViewer.ViewModel;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace LogViewer.Services
{
    public class HttpProcessor : IComponentProcessor
    {
        public void ReadData(string path, string componentName, ref BackgroundWorker asyncWorker, StoreTypes storeType)
        {
            HttpListener web = null;
            try
            {
                web = new HttpListener();
                web.Prefixes.Add(path);
                web.Start();

                do
                {
                    var ctx = web.GetContext();
                    var req = ctx.Request;
                    var data = new StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd();

                    if (ProcessorMonitorContainer.ComponentStopper[componentName])
                    {
                        ProcessorMonitorContainer.ComponentStopper[componentName] = false;
                        break;
                    }

                    // parse json into an Entry array
                    var ents = JsonConvert.DeserializeObject<LogEntries>(data);

                    foreach (var ent in ents.Entries)
                    {
                        var lvlType = Levels.GetLevelTypeFromString(ent.Level);

                        // Save type validation (Disk or RAM)
                        if (storeType == StoreTypes.Disk)
                        {
                            // insert into db
                            DbProcessor.WriteOne(componentName, new Entry
                            {
                                Timestamp = ent.Timestamp,
                                RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
                                LevelType = (int)lvlType,
                                Component = componentName
                            });

                            // increment counters
                            MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(lvlType);
                            MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(Levels.LevelTypes.All);
                        }
                        else
                        {
                            // insert into dictionary
                            MessageContainer.RAM.HttpMessages[componentName].Value.Add(new Entry
                            {
                                Timestamp = ent.Timestamp,
                                RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
                                LevelType = (int)lvlType,
                                Component = componentName
                            });
                        }
                    }
                } while (!ProcessorMonitorContainer.ComponentStopper[componentName]);

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (web != null)
                {
                    if (web.IsListening) {
                        web.Stop();
                    }
                    web = null;
                }
                asyncWorker.CancelAsync();
            }
        }
    }

}
