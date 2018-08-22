using System;
using System.ComponentModel;
using System.Windows.Media;

namespace LogViewer.Model
{
    public static class Levels
    {
        public enum LevelTypes
        {
            All,

            [Description("VRB")]
            Verbose,

            [Description("DBG")]
            Debug,

            [Description("INF")]
            Information,

            [Description("WRN")]
            Warning,

            [Description("ERR")]
            Error,

            [Description("FTL")]
            Fatal
        }

        public static Brush GetLevelColor(LevelTypes level)
        {
            switch (level)
            {
                case LevelTypes.Verbose:
                    return Brushes.Purple;
                case LevelTypes.Debug:
                    return Brushes.Gray;
                case LevelTypes.Information:
                    return Brushes.Blue;
                case LevelTypes.Warning:
                    return Brushes.Orange;
                case LevelTypes.Error:
                    return Brushes.Red;
                case LevelTypes.Fatal:
                    return Brushes.Green;
                case LevelTypes.All:
                    return Brushes.Black;
            }
            return Brushes.Black;
        }

        public static LevelTypes GetLevelTypeFromString(string levelString)
        {
            foreach (var item in Enum.GetNames(typeof(LevelTypes)))
            {
                var field = typeof(LevelTypes).GetField(item);
                var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var desc = (attr == null || attr.Length == 0) ? string.Empty : (attr[0] as DescriptionAttribute).Description;

                if (item == levelString || desc == levelString) {
                    return (LevelTypes)Enum.Parse(typeof(LevelTypes), item);
                }
            }
            return LevelTypes.All;
        }

        public static string GetLevelStringFromType(LevelTypes level)
        {
            return Enum.GetName(typeof(LevelTypes), level);
        }
    }
}
