using LogViewer.Abstractions;
using LogViewer.Types;

namespace LogViewer.Model
{
    public class StoreProcessorsVM : IStoreProcessor
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public IEnrichedCommand ExecutionCommand { get; set; }
        public StoreTypes ExecutionParameter { get; set; }
    }
}