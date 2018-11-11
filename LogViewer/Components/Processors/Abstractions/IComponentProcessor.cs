using System.ComponentModel;
using LogViewer.StoreProcessors.Abstractions;

namespace LogViewer.Components.Processors.Abstractions
{
    public interface IComponentProcessor
    {
        void ReadData(string path, string componentName, IDbProcessor dbProcessor);
    }
}
