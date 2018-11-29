using System.Windows.Media;
using LogViewer.Types;

namespace LogViewer.Abstractions
{
    public interface ILevels
    {
        LevelTypes LevelType { get; set; }
        string Text { get; }
        int Counter { get; set; }
        Brush TextColor { get; }
        bool IsSelected { get; set; }
    }
}
