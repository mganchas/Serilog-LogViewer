using System;
using System.IO;
using System.Text;
using LogViewer.Abstractions;
using LogViewer.Containers;
using LogViewer.Factories;
using LogViewer.Utilities;

namespace LogViewer.Listeners
{
    public sealed class FileListener : IComponentProcessor
    {
        public void ReadData(string path, string componentName, IDbProcessor dbProcessor)
        {
            using (var sr = new StreamReader(path))
            {
                try
                {
                    string line = null;
                    var sb = new StringBuilder();

                    while ((line = sr.ReadLine()) != null && !ProcessorMonitorContainer.ComponentStopper[componentName])
                    {
                        if (ProcessorMonitorContainer.ComponentStopper[componentName])
                        {
                            ProcessorMonitorContainer.ComponentStopper[componentName] = false;
                            break;
                        }

                        var levelInit = line.IndexOf('[');
                        var levelEnd = line.IndexOf(']');
                        var split = line.Split(' ');

                        var isValid = !(split.Length < 5 || levelInit == -1 || levelEnd == -1 || levelEnd - levelInit != 4);

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
                                var prevLines = sb.ToString().TrimEnd();
                                var lvlRaw = prevLines.Substring(levelInit + 1, 3);
                                var lvlType = LevelTypesHelper.GetLevelTypeFromString(lvlRaw);

                                // save entry
                                IEntry newEntry = EntryFactory.CreateNewEntry
                                (
                                    DateTime.Parse(prevLines.Substring(0, 29)),
                                    prevLines.Substring(levelEnd + 1),
                                    (int) lvlType,
                                    componentName
                                );
                                dbProcessor.WriteOne(componentName, newEntry);

                                // remove previous event
                                sb.Clear();

                                // add current event to queue
                                sb.AppendLine(line);
                            }
                        }
                    }
                }
                finally
                {
                    sr?.Close();
                }
            }

            GC.Collect();
        }
    }
}