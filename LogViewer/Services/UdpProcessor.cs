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

        public override void ReadData(ref CancellationTokenSource cancelToken, ref BackgroundWorker asyncWorker)
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

                while (true)
                {
                    if (cancelToken.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);
                    var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    var ent = JsonConvert.DeserializeObject<Entry>(data);

                    // insert into dictionary
                    MessageContainer.UdpMessages[componentName].Add(new Entry
                    {
                        Timestamp = ent.Timestamp,
                        RenderedMessage = $"{ent.RenderedMessage} {ent.Message} {ent.Exception}",
                        LevelType = Levels.GetLevelTypeFromString(ent.Level),
                        Component = componentName
                    });
                }
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
