using LogViewer.Model;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;

namespace LogViewer.Services
{
    public sealed class FileProcessor : BaseDataReader
    {
        public FileProcessor(string path, string componentName) : base(path, componentName)
        {
        }

        public override void ReadData(ref CancellationTokenSource cancelToken, ref BackgroundWorker asyncWorker, StoreTypes storeType)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                try
                {
                    string line = null;
                    StringBuilder sb = new StringBuilder();
                    bool isValid = false;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (cancelToken.Token.IsCancellationRequested || asyncWorker.CancellationPending)
                        {
                            break;
                        }

                        var level_init = line.IndexOf('[');
                        var level_end = line.IndexOf(']');
                        var split = line.Split(' ');

                        isValid = !(split.Length < 5 || level_init == -1 || level_end == -1 || (level_end - level_init) != 4);

                        if (!isValid) // add to queue
                        {
                            sb.AppendLine(line);
                        }
                        else
                        {
                            if (sb.Length == 0) // first valid line
                            {
                                sb.AppendLine(line);
                            }
                            else
                            {
                                // previous event lines
                                string prevLines = sb.ToString().TrimEnd();
                                string lvlRaw = prevLines.Substring(level_init + 1, 3);
                                var lvlType = Levels.GetLevelTypeFromString(lvlRaw);

                                // Save type validation (Disk or RAM)
                                if (storeType == StoreTypes.Disk)
                                {
                                    // insert into db
                                    DbProcessor.WriteOne(componentName, new Entry
                                    {
                                        Timestamp = DateTime.Parse(prevLines.Substring(0, 29)),
                                        RenderedMessage = prevLines.Substring(level_end + 1),
                                        LevelType = (int)lvlType,
                                        Component = componentName
                                    });

                                    // increment counters
                                    MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(lvlType);
                                    MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(Levels.LevelTypes.All);
                                }
                                else
                                {
                                    MessageContainer.RAM.FileMessages[componentName].Add(new Entry
                                    {
                                        Timestamp = DateTime.Parse(prevLines.Substring(0, 29)),
                                        RenderedMessage = prevLines.Substring(level_end + 1),
                                        LevelType = (int)lvlType,
                                        Component = componentName
                                    });
                                }
                                
                                // remove previous event
                                sb.Clear();

                                // add current event to queue
                                sb.AppendLine(line);
                            }
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
                    sr?.Close();
                    asyncWorker.CancelAsync();
                }
            }
            GC.Collect();
        }

        public static bool Exists(string file)
        {
            return File.Exists(file);
        }
    }
}
