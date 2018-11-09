using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using LogViewer.Components.Levels.Helpers;
using LogViewer.Components.Processors.Abstractions;
using LogViewer.Entries;
using LogViewer.Services.Abstractions;
using LogViewer.Structures.Containers;
using Newtonsoft.Json;

namespace LogViewer.Components.Processors
{
    public class HttpProcessor : IComponentProcessor
    {
        public void ReadData(string path, string componentName, ref BackgroundWorker asyncWorker, IDbProcessor dbProcessor)
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
                    var ents = JsonConvert.DeserializeObject<Entry[]>(data);

                    foreach (var ent in ents)
                    {
                        var lvlType = LevelTypesHelper.GetLevelTypeFromString(ent.Level);

                        // Save type validation (Disk or RAM)
//                        if (storeType == StoreTypes.MongoDB)
//                        {
//                            // insert into db
//                            new MongoDBProcessor(componentName).WriteOne(new Entry
//                            {
//                                Timestamp = ent.Timestamp,
//                                RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
//                                LevelType = (int)lvlType,
//                                Component = componentName
//                            });
//
//                            // increment counters
//                            MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(lvlType);
//                            MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(LevelTypes.All);
//                        }
//                        else
//                        {
//                            // insert into dictionary
//                            MessageContainer.RAM.HttpMessages[componentName].Value.Add(new Entry
//                            {
//                                Timestamp = ent.Timestamp,
//                                RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
//                                LevelType = (int)lvlType,
//                                Component = componentName
//                            });
//                        }
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
