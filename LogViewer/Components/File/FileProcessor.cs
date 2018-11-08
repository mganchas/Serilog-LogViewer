using LogViewer.Containers;
using LogViewer.Model;
using LogViewer.Services.Abstractions;
using LogViewer.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace LogViewer.Services
{
    public sealed class FileProcessor : IComponentProcessor
    {
        public void ReadData(string path, string componentName, ref BackgroundWorker asyncWorker, StoreTypes storeType)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                try
                {
                    string line = null;
                    StringBuilder sb = new StringBuilder();
                    bool isValid = false;

                    while ((line = sr.ReadLine()) != null && !ProcessorMonitorContainer.ComponentStopper[componentName])
                    {
                        if (ProcessorMonitorContainer.ComponentStopper[componentName])
                        {
                            ProcessorMonitorContainer.ComponentStopper[componentName] = false;
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
                                if (storeType == StoreTypes.MongoDB)
                                {
                                    // insert into db
                                    new MongoDbProcessor(componentName).WriteOne(new Entry
                                    {
                                        Timestamp = DateTime.Parse(prevLines.Substring(0, 29)),
                                        RenderedMessage = prevLines.Substring(level_end + 1),
                                        LevelType = (int)lvlType,
                                        Component = componentName
                                    });

                                    // increment counters
                                    MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(lvlType);
                                    MessageContainer.Disk.ComponentCounters[componentName].IncrementCounter(LevelTypes.All);
                                }
                                else
                                {
                                    MessageContainer.RAM.FileMessages[componentName].Value.Add(new Entry
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
                    throw e;
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
