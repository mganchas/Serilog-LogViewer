using System.Net;
using System.Net.Sockets;
using System.Text;
using LogViewer.Abstractions;
using LogViewer.Containers;
using LogViewer.Factories;
using LogViewer.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogViewer.Listeners
{
    public class TcpListener : IComponentProcessor
    {
        public void ReadData(string path, string componentName, IDbProcessor dbProcessor)
        {
            System.Net.Sockets.TcpListener server = null;

            try
            {
                // get the address and port to listen
                var splitPath = path.Split(':');
                var localAddr = IPAddress.Parse(splitPath[0]);
                var port = int.Parse(splitPath[1]);

                // Set the TcpListener port and address.
                server = new System.Net.Sockets.TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                var bytes = new byte[256];
                NetworkStream stream = null;
                TcpClient client = null;
                int i;

                // Enter the listening loop.
                do
                {
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    client = server.AcceptTcpClient();

                    // Get a stream object for reading and writing
                    stream = client.GetStream();

                    if (ProcessorMonitorContainer.ComponentStopper[componentName])
                    {
                        ProcessorMonitorContainer.ComponentStopper[componentName] = false;
                        break;
                    }

                    // Loop to receive all the data sent by the client.
                    var sb = new StringBuilder();
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        sb.Append(Encoding.ASCII.GetString(bytes, 0, i));
                    }

                    // open json array
                    // Note: the objects received from Serilog don't arrive with a json array format, so we have to force it
                    sb.Insert(0, '[');

                    // close json array
                    sb.Append(']');

                    // add comma to each json object
                    sb.Replace(@"{""timestamp""", @",{""timestamp""");

                    // replace first comma with empty space
                    sb[1] = ' ';

                    // parse json into an Entry array
                    var ents = JArray.Parse(sb.ToString());

                    foreach (var item in ents)
                    {
                        var ent = JsonConvert.DeserializeObject<IEntry>(item.ToString());
                        var lvlType = LevelTypesHelper.GetLevelTypeFromString(ent.Level);

                        IEntry newEntry = EntryFactory.CreateNewEntry
                        (
                            ent.Timestamp,
                            $"{ent.RenderedMessage} {ent.Message}",
                            (int) lvlType,
                            componentName,
                            ent.Exception
                        );
                        dbProcessor.WriteOne(componentName, newEntry);
                    }

                    // Shutdown and end connection
                    client.Close();
                } while (!ProcessorMonitorContainer.ComponentStopper[componentName]);
            }
            finally
            {
                // Stop listening for new clients.
                server?.Stop();
            }
        }
    }
}