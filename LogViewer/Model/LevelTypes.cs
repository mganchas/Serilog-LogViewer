﻿using System;
using System.Collections.Generic;
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

        private static Dictionary<LevelTypes, SolidColorBrush> LevelColors => new Dictionary<LevelTypes, SolidColorBrush>
        {
            { LevelTypes.Verbose, Brushes.Purple },
            { LevelTypes.Debug, Brushes.Gray },
            { LevelTypes.Information, Brushes.Blue },
            { LevelTypes.Warning, Brushes.Orange },
            { LevelTypes.Error, Brushes.Red },
            { LevelTypes.Fatal, Brushes.Green },
            { LevelTypes.All, Brushes.Black },
        };

        public static Brush GetLevelColor(LevelTypes level)
        {
            return LevelColors[level];
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
