using System.Net;
using System.Net.Sockets;
using System.Text;
using LogViewer.Abstractions;
using LogViewer.Containers;
using LogViewer.Factories;
using LogViewer.Utilities;
using Newtonsoft.Json;

namespace LogViewer.Listeners
{
    public sealed class UdpListener : IComponentProcessor
    {
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
                    var ent = JsonConvert.DeserializeObject<IEntry>(data);
                    var lvlType = LevelTypesHelper.GetLevelTypeFromString(ent.Level);

                    // save entry
                    IEntry newEntry = EntryFactory.CreateNewEntry
                    (
                        ent.Timestamp,
                        $"{ent.RenderedMessage} {ent.Message}",
                        (int) lvlType,
                        componentName,
                        ent.Exception
                    );
                    dbProcessor.WriteOne(componentName, newEntry);
                } while (!ProcessorMonitorContainer.ComponentStopper[componentName]);
            }
            finally
            {
                listener?.Close();
            }
        }
    }
}