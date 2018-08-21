﻿using LiveViewer.Model;
using LiveViewer.Types;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;

namespace LiveViewer.Services
{
    public sealed class FileProcessor
    {
        private readonly string filePath;
        private readonly string componentName;

        public FileProcessor(string filePath, string componentName)
        {
            this.filePath = filePath;
            this.componentName = componentName;
        }

        public void ReadFile(CancellationToken cancelToken, ref BackgroundWorker asyncWorker)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                int read = 0;
                string line = null;
                StringBuilder sb = new StringBuilder();
                bool isValid = false;

                while ((line = sr.ReadLine()) != null)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
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

                            // insert into dictionary
                            MessageContainer.FileMessages[componentName].Add(new Entry
                            {
                                Timestamp = DateTime.Parse(prevLines.Substring(0, 29)),
                                RenderedMessage = prevLines.Substring(level_end + 1),
                                LevelType = Levels.GetLevelTypeFromString(lvlRaw),
                                Component = componentName
                            });

                            // remove previous event
                            sb.Clear();

                            // add current event to queue
                            sb.AppendLine(line);
                        }
                    }

                    read++;
                    //if ((read % 10000) == 0) { GC.Collect(); }
                }

                sr.Close();
                asyncWorker.CancelAsync();
            }
        }

        public static bool Exists(string file)
        {
            return File.Exists(file);
        }
    }
}
