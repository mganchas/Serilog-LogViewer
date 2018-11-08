using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services.Abstractions;
using LogViewer.ViewModel;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LogViewer.Services
{
    public sealed class UdpProcessor : IComponentProcessor
    {
        public void ReadData(string path, string componentName, ref BackgroundWorker asyncWorker, StoreTypes storeType)
        {
            UdpClient listener = null;
            try
            {
                // get the address and port to listen
                var splitPath = path.Split(':');
                var localAddr = IPAddress.Parse(splitPath[0]);
                var port = int.Parse(splitPath[1]);

                listener = new UdpClient(port);
                IPEndPoint groupEP = new IPEndPoint(localAddr, port);

                do
                {
                    byte[] bytes = listener.Receive(ref groupEP);

                    if (ProcessorMonitorContainer.ComponentStopper[componentName])
                    {
                        ProcessorMonitorContainer.ComponentStopper[componentName] = false;
                        break;
                    }

                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    var ent = JsonConvert.DeserializeObject<Entry>(data);
                    var lvlType = Levels.GetLevelTypeFromString(ent.Level);

                    // Save type validation (Disk or RAM)
                    if (storeType == StoreTypes.MongoDB)
                    {
                        // insert into db
                        new MongoDBProcessor(componentName).WriteOne(new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
                            LevelType = (int)lvlType,
                            Component = componentName
                        });

                        // increment counters
                        MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(lvlType);
                        MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(LevelTypes.All);
                    }
                    else
                    {
                        // insert into dictionary
                        MessageContainer.RAM.UdpMessages[componentName].Value.Add(new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
                            LevelType = (int)lvlType,
                            Component = componentName
                        });
                    }
                }
                while (!ProcessorMonitorContainer.ComponentStopper[componentName]);
            }
            finally
            {
                listener?.Close();
                asyncWorker.CancelAsync();
            }
        }
    }
}
