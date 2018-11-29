using System;
using System.Collections.Generic;
using System.Windows.Media;
using LogViewer.Abstractions;
using LogViewer.Extensions;
using LogViewer.Types;

namespace LogViewer.Model
{
    public class LevelsVM : PropertyChangesNotifier, ILevels
    {
        public static Dictionary<LevelTypes, LevelsVM> LevelVMList { get; } = new Dictionary<LevelTypes, LevelsVM>
        {
            {LevelTypes.All, new LevelsVM(LevelTypes.All, true)},
            {LevelTypes.Verbose, new LevelsVM(LevelTypes.Verbose)},
            {LevelTypes.Debug, new LevelsVM(LevelTypes.Debug)},
            {LevelTypes.Information, new LevelsVM(LevelTypes.Information)},
            {LevelTypes.Warning, new LevelsVM(LevelTypes.Warning)},
            {LevelTypes.Error, new LevelsVM(LevelTypes.Error)},
            {LevelTypes.Fatal, new LevelsVM(LevelTypes.Fatal)}
        };
        
        public static Dictionary<LevelTypes, SolidColorBrush> LevelColors { get; } = new Dictionary<LevelTypes, SolidColorBrush>
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

        public LevelTypes LevelType { get; set; }

        private string _text;
        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(_text))
                {
                    _text = Enum.GetName(typeof(LevelTypes), LevelType);
                    NotifyPropertyChanged();
                }
                return _text;
            }
        }

        private int _counter;
        public int Counter
        {
            get => _counter;
            set { _counter = value; NotifyPropertyChanged(); }
        }

        private Brush _textColor;
        public Brush TextColor
        {
            get
            {
                if (_textColor == null)
                {
                    _textColor = LevelType.GetLevelColor(LevelColors);
                    NotifyPropertyChanged();
                }
                return _textColor;
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; NotifyPropertyChanged(); }
        }

        public LevelsVM(LevelTypes level) => LevelType = level;
        public LevelsVM(LevelTypes level, bool isSelected) : this(level) => IsSelected = isSelected;
    }
}
