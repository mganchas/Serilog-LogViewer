using System.ComponentModel;
using LogViewer.Services.Abstractions;

namespace LogViewer.Components.Processors.Abstractions
{
    public interface IComponentProcessor
    {
        void ReadData(string path, string componentName, ref BackgroundWorker asyncWorker, IDbProcessor dbProcessor);
    }
}
