using System;

namespace LogViewer.Entries.Abstractions
{
    public interface IEntry
    {
        DateTimeOffset Timestamp { get; set; }
        
        string Level { get; set; }
        int LevelType { get; set; }
        
        string Message { get; set; }
        string RenderedMessage { get; set; }
        
        string Exception { get; set; }
        
        string Component { get; set; }
    }
}