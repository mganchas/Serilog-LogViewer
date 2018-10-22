using LogViewer.Model;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LogViewer.Services
{
    public sealed class UdpProcessor : BaseDataReader
    {
        public UdpProcessor(string path, string componentName) : base(path, componentName)
        {
        }

        public override void ReadData(ref CancellationTokenSource cancelToken, ref BackgroundWorker asyncWorker, StoreTypes storeType)
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

                    if (ProcessorMonitor.ComponentStopper[componentName])
                    {
                        ProcessorMonitor.ComponentStopper[componentName] = false;
                        break;
                    }

                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    var ent = JsonConvert.DeserializeObject<Entry>(data);
                    var lvlType = Levels.GetLevelTypeFromString(ent.Level);

                    // Save type validation (Disk or RAM)
                    if (storeType == StoreTypes.Disk)
                    {
                        // insert into db
                        DbProcessor.WriteOne(componentName, new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
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
                        MessageContainer.RAM.UdpMessages[componentName].Value.Add(new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
                            LevelType = (int)lvlType,
                            Component = componentName
                        });
                    }
                }
                while (!ProcessorMonitor.ComponentStopper[componentName]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                listener?.Close();
                cancelToken.Cancel();
            }
        }
    }
}
