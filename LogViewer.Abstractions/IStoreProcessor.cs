using LogViewer.Types;

namespace LogViewer.Abstractions
{
    public interface IStoreProcessor
    {
        string Name { get; set; }
        string Image { get; set; }
        IEnrichedCommand ExecutionCommand { get; set; }
        StoreTypes ExecutionParameter { get; set; }
    }
}
