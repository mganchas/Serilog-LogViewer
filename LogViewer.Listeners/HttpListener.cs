using System.IO;
using LogViewer.Abstractions;
using LogViewer.Containers;
using LogViewer.Factories;
using LogViewer.Utilities;
using Newtonsoft.Json;

namespace LogViewer.Listeners
{
    public class HttpListener : IComponentProcessor
    {
        public void ReadData(string path, string componentName, IDbProcessor dbProcessor)
        {
            System.Net.HttpListener web = null;
            try
            {
                web = new System.Net.HttpListener();
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
                    var entries = JsonConvert.DeserializeObject<IEntry[]>(data);

                    foreach (var entry in entries)
                    {
                        var lvlType = LevelTypesHelper.GetLevelTypeFromString(entry.Level);

                        IEntry newEntry = EntryFactory.CreateNewEntry
                        (
                            entry.Timestamp,
                            $"{entry.RenderedMessage}",
                            (int) lvlType,
                            componentName,
                            entry.Exception
                        );
                        dbProcessor.WriteOne(componentName, newEntry);
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