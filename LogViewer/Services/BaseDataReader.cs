using System.ComponentModel;
using System.Threading;

namespace LogViewer.Services
{
    public abstract class BaseDataReader
    {
        protected readonly string path;
        protected readonly string componentName;

        public BaseDataReader(string path, string componentName)
        {
            this.path = path;
            this.componentName = componentName;
        }

        public abstract void ReadData(ref CancellationTokenSource cancelToken, ref BackgroundWorker asyncWorker);
    }
}
