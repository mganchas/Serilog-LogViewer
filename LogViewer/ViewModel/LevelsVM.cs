using LogViewer.Model;
using System;
using System.Windows.Media;
using static LogViewer.Model.Levels;

namespace LogViewer.ViewModel
{
    public class LevelsVM : PropertyChangesNotifier
    {
        public LevelTypes LevelType { get; private set; }

        private string _text;
        public string Text
        {
            get
            {
                if (String.IsNullOrEmpty(_text))
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
            get { return _counter; }
            set { _counter = value; NotifyPropertyChanged(); }
        }

        private Brush _textColor;
        public Brush TextColor
        {
            get
            {
                if (_textColor == null)
                {
                    _textColor = Levels.GetLevelColor(LevelType);
                    NotifyPropertyChanged();
                }
                return _textColor;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(); }
        }

        public LevelsVM(LevelTypes level) => LevelType = level;
    }
}
