namespace LogViewer.Abstractions
{
    public interface IComponentProcessor
    {
        void ReadData(string path, string componentName, IDbProcessor dbProcessor);
    }
}