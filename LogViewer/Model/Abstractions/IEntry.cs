namespace LogViewer.Model.Abstractions
{
    public interface IEntry
    {
        string RenderedMessage { get; set; }
        int LevelType { get; set; }
    }
}