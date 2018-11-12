using System.Windows.Input;

namespace LogViewer.StoreProcessors
{
    public class StoreProcessorsVM
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public ICommand ExecutionCommand { get; set; }
        public StoreTypes ExecutionParameter { get; set; }
    }
}