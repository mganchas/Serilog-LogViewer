using LogViewer.Model;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace LogViewer.Services
{
    public class HttpProcessor : BaseDataReader
    {
        public HttpProcessor(string path, string componentName) : base(path, componentName)
        {
        }

        public override void ReadData(ref CancellationTokenSource cancelToken, ref BackgroundWorker asyncWorker, StoreTypes storeType)
        {
            HttpListener web = null;
            try
            {
                web = new HttpListener();
                web.Prefixes.Add(path);
                web.Start();

                while (true)
                {
                    if (cancelToken.Token.IsCancellationRequested || asyncWorker.CancellationPending)
                    {
                        break;
                    }

                    var ctx = web.GetContext();
                    var req = ctx.Request;
                    var data = new StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd();

                    // parse json into an Entry array
                    var ents = JsonConvert.DeserializeObject<LogEntries>(data);

                    foreach (var ent in ents.Entries)
                    {
                        // insert into dictionary
                        MessageContainer.RAM.HttpMessages[componentName].Add(new Entry
                        {
                            Timestamp = ent.Timestamp,
                            RenderedMessage = $"{ent.RenderedMessage} {ent.Exception}",
                            LevelType = (int)Levels.GetLevelTypeFromString(ent.Level),
                            Component = componentName
                        });
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                web?.Stop();
                cancelToken.Cancel();
            }
        }
    }

}
