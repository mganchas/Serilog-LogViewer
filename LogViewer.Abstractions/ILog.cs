using System;

namespace LogViewer.Abstractions
{
    public interface ILog
    {
        string TypeImage { get; }
        string VisualTimestamp { get; }
        DateTimeOffset Timestamp { get; set; }
        string Message { get; set; }
    }
}