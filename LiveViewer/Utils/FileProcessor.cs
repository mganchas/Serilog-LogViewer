using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using static LiveViewer.ViewModel.LogEventsVM;

namespace LiveViewer.Utils
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
            int read = 0;
            using (StreamReader sr = new StreamReader(filePath))
            {
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
                            string prevLines = sb.ToString();
                            
                            // convert previous event to class obj
                            var logEvent = new LogEvent
                            {
                                Timestamp = DateTime.Parse(prevLines.Substring(0, 29)),
                                Level = prevLines.Substring(level_init + 1, 3),
                                RenderedMessage = prevLines.Substring(level_end + 1)
                            };
                            MessageContainer.FileMessages[componentName].Add(logEvent);

                            // remove previous event
                            sb.Clear();

                            // add current event to queue
                            sb.AppendLine(line);
                        }
                    }

                    read++;
                    if ((read % 10000) == 0) { GC.Collect(); }
                }
                asyncWorker.CancelAsync();
            }
        }

        public static bool Exists(string file)
        {
            return File.Exists(file);
        }
    }
}
