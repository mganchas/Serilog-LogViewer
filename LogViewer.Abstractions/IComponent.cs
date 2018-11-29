namespace LogViewer.Abstractions
{
    public interface ICustomComponent
    {
        string Name { get; }
        string Path { get; }
        bool IsRunning { get; }

        IEnrichedCommand StopListenerCommand { get; set; }
        IEnrichedCommand CleanUpCommand { get; set; }

        void RemoveComponent();
    }
}