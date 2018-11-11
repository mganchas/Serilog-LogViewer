using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LogViewer.Components.Levels.Helpers;
using LogViewer.Components.Processors.Abstractions;
using LogViewer.Entries;
using LogViewer.StoreProcessors.Abstractions;
using LogViewer.Structures.Containers;
using Newtonsoft.Json;

namespace LogViewer.Components.Processors
{
    public sealed class UdpProcessor : IComponentProcessor
    {
        private static readonly Lazy<UdpProcessor> _lazy = new Lazy<UdpProcessor>(() => new UdpProcessor());
        public static UdpProcessor Instance => _lazy.Value;

        public UdpProcessor()
        {
        }
        
        public void ReadData(string path, string componentName, IDbProcessor dbProcessor)
        {
            UdpClient listener = null;
            try
            {
                // get the address and port to listen
                var splitPath = path.Split(':');
                var localAddr = IPAddress.Parse(splitPath[0]);
                var port = int.Parse(splitPath[1]);

                listener = new UdpClient(port);
                var groupEP = new IPEndPoint(localAddr, port);

                do
                {
                    var bytes = listener.Receive(ref groupEP);

                    if (ProcessorMonitorContainer.ComponentStopper[componentName])
                    {
                        ProcessorMonitorContainer.ComponentStopper[componentName] = false;
                        break;
                    }

                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    var ent = JsonConvert.DeserializeObject<Entry>(data);
                    var lvlType = LevelTypesHelper.GetLevelTypeFromString(ent.Level);

                    // save entry
                    dbProcessor.WriteOne(new Entry
                    {
                        Timestamp = ent.Timestamp,
                        RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
                        LevelType = (int) lvlType,
                        Component = componentName
                    });
                } while (!ProcessorMonitorContainer.ComponentStopper[componentName]);
            }
            finally
            {
                listener?.Close();
            }
        }
    }
}