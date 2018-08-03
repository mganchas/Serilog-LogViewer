﻿using System;
using System.Windows.Media;

namespace LiveViewer.Utils
{
    public static class Levels
    {
        public enum LevelTypes
        {
            All,
            Debug,
            Information,
            Warning,
            Error,
            Fatal
        }

        public static Brush GetLevelColor(LevelTypes level)
        {
            switch (level)
            {
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
                if (item == levelString) {
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
