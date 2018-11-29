using LogViewer.Types;
using System;
using System.ComponentModel;

namespace LogViewer.Utilities
{
    public static class LevelTypesHelper
    {
        public static LevelTypes GetLevelTypeFromString(string levelString)
        {
            foreach (var item in Enum.GetNames(typeof(LevelTypes)))
            {
                var field = typeof(LevelTypes).GetField(item);
                var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var desc = attr == null || attr.Length == 0
                    ? string.Empty
                    : (attr[0] as DescriptionAttribute)?.Description;

                if (item == levelString || desc == levelString)
                {
                    return (LevelTypes)Enum.Parse(typeof(LevelTypes), item);
                }
            }

            return LevelTypes.All;
        }
    }
}
