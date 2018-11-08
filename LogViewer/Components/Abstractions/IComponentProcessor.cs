using LogViewer.Model;
using System.ComponentModel;

namespace LogViewer.Services.Abstractions
{
    public interface IComponentProcessor
    {
        void ReadData(string path, string componentName, ref BackgroundWorker asyncWorker, StoreTypes storeType);
    }
}
