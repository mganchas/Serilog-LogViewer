using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using LogViewer.Components.Processors.Abstractions;
using LogViewer.Entries;
using LogViewer.Levels.Helpers;
using LogViewer.StoreProcessors.Abstractions;
using LogViewer.Structures.Containers;
using Newtonsoft.Json;

namespace LogViewer.Components.Processors
{
    public class HttpProcessor : IComponentProcessor
    {
        private static readonly Lazy<HttpProcessor> _lazy = new Lazy<HttpProcessor>(() => new HttpProcessor());
        public static HttpProcessor Instance => _lazy.Value;

        public HttpProcessor()
        {
        }
        
        public void ReadData(string path, string componentName, IDbProcessor dbProcessor)
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

                        // save entry
                        dbProcessor.WriteOne(componentName, new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
                            LevelType = (int) lvlType,
                            Component = componentName
                        });
                    }
                } while (!ProcessorMonitorContainer.ComponentStopper[componentName]);
            }
            finally
            {
                if (web != null && web.IsListening)
                {
                    web.Stop();
                }
            }
        }
    }
}