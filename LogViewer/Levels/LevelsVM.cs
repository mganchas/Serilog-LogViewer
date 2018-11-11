using System;
using System.Windows.Media;
using LogViewer.Components.Levels.Helpers;
using LogViewer.ViewModelHelpers;

namespace LogViewer.Components.Levels
{
    public class LevelsVM : PropertyChangesNotifier
    {
        public LevelTypes LevelType { get; private set; }

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
                    _textColor = LevelTypesHelper.GetLevelColor(LevelType);
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
