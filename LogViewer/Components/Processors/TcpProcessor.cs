using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LogViewer.Components.Levels.Helpers;
using LogViewer.Components.Processors.Abstractions;
using LogViewer.Entries;
using LogViewer.StoreProcessors.Abstractions;
using LogViewer.Structures.Containers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogViewer.Components.Processors
{
    public class TcpProcessor : IComponentProcessor
    {
        private static readonly Lazy<TcpProcessor> _lazy = new Lazy<TcpProcessor>(() => new TcpProcessor());
        public static TcpProcessor Instance => _lazy.Value;

        public TcpProcessor()
        {
        }
        
        public void ReadData(string path, string componentName, IDbProcessor dbProcessor)
        {
            TcpListener server = null;

            try
            {
                // get the address and port to listen
                var splitPath = path.Split(':');
                var localAddr = IPAddress.Parse(splitPath[0]);
                var port = int.Parse(splitPath[1]);

                // Set the TcpListener port and address.
                server = new TcpListener(localAddr, port);

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
                        var ent = JsonConvert.DeserializeObject<Entry>(item.ToString());
                        var lvlType = LevelTypesHelper.GetLevelTypeFromString(ent.Level);

                        // save entry
                        dbProcessor.WriteOne(new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
                            LevelType = (int) lvlType,
                            Component = componentName
                        });
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