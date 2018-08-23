using LogViewer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public sealed class UdpProcessor : BaseDataReader
    {
        public UdpProcessor(string path, string componentName) : base(path, componentName)
        {
        }

        public override void ReadData(CancellationToken cancelToken, ref BackgroundWorker asyncWorker)
        {
            UdpClient listener = null;
            try
            {
                // get the address and port to listen
                var splitPath = path.Split(':');
                var localAddr = IPAddress.Parse(splitPath[0]);
                var port = int.Parse(splitPath[1]);

                listener = new UdpClient(port);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);
                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    var ent = JsonConvert.DeserializeObject<Entry>(data);

                    // insert into dictionary
                    MessageContainer.UdpMessages[componentName].Add(new Entry
                    {
                        Timestamp = ent.Timestamp,
                        RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
                        LevelType = Levels.GetLevelTypeFromString(ent.Level),
                        Component = componentName
                    });

                    Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener?.Close();
            }
        }
    }
}
