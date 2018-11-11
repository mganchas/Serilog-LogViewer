using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using LogViewer.Structures;
using LogViewer.Structures.Collections;

namespace LogViewer.Components.Levels.Helpers
{
    public static class LevelTypesHelper
    {
        private static Dictionary<LevelTypes, SolidColorBrush> LevelColors { get; } = new Dictionary<LevelTypes, SolidColorBrush>
            {
                {LevelTypes.Verbose, Brushes.Purple},
                {LevelTypes.Debug, Brushes.Gray},
                {LevelTypes.Information, Brushes.Blue},
                {LevelTypes.Warning, Brushes.Orange},
                {LevelTypes.Error, Brushes.Red},
                {LevelTypes.Fatal, Brushes.Green},
                {LevelTypes.All, Brushes.Black}
            };

        public static ObservableCounterDictionary<LevelTypes> LevelTypesCounterList { get; } = new ObservableCounterDictionary<LevelTypes>
        {
            {LevelTypes.All, 0},
            {LevelTypes.Verbose, 0},
            {LevelTypes.Debug, 0},
            {LevelTypes.Information, 0},
            {LevelTypes.Warning, 0},
            {LevelTypes.Error, 0},
            {LevelTypes.Fatal, 0}
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
                var desc = attr == null || attr.Length == 0
                    ? string.Empty
                    : (attr[0] as DescriptionAttribute)?.Description;

                if (item == levelString || desc == levelString) {
                    return (LevelTypes) Enum.Parse(typeof(LevelTypes), item);
                }
            }

            return LevelTypes.All;
        }
    }
}