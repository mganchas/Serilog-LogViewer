using LogViewer.Types;
using System.Collections.Generic;
using System.Windows.Media;

namespace LogViewer.Extensions
{
    public static class LevelTypesExtensions
    {
        public static Brush GetLevelColor(this LevelTypes level, Dictionary<LevelTypes, SolidColorBrush> levelColors)
        {
            return levelColors[level];
        }
    }
}