using LogViewer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LogViewer.Services
{
    public class TcpProcessor : BaseDataReader
    {
        public TcpProcessor(string path, string componentName) : base(path, componentName)
        {
        }

        public override void ReadData(CancellationToken cancelToken, ref BackgroundWorker asyncWorker)
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
                Byte[] bytes = new Byte[256];
                NetworkStream stream = null;
                TcpClient client = null;
                int i;

                // Enter the listening loop.
                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    client = server.AcceptTcpClient();

                    // Get a stream object for reading and writing
                    stream = client.GetStream();

                    // Loop to receive all the data sent by the client.
                    StringBuilder sb = new StringBuilder();
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

                        // insert into dictionary
                        MessageContainer.TcpMessages[componentName].Add(new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
                            LevelType = Levels.GetLevelTypeFromString(ent.Level),
                            Component = componentName
                        });
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e.Message);
            }
            finally
            {               
                // Stop listening for new clients.
                server?.Stop();
            }
        }
    }
}
