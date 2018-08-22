using LogViewer.Model;
using System;
using System.Windows.Media;
using static LogViewer.Model.Levels;

namespace LogViewer.ViewModel
{
    public class LevelsVM : BaseVM
    {
        public LevelTypes LevelType { get; private set; }

        private string text;
        public string Text
        {
            get
            {
                if (String.IsNullOrEmpty(text))
                {
                    text = Enum.GetName(typeof(LevelTypes), LevelType);
                    NotifyPropertyChanged();
                }
                return text;
            }
        }

        private int counter;
        public int Counter
        {
            get { return counter; }
            set { counter = value; NotifyPropertyChanged(); }
        }

        private Brush textColor;
        public Brush TextColor
        {
            get
            {
                if (textColor == null)
                {
                    textColor = Levels.GetLevelColor(LevelType);
                    NotifyPropertyChanged();
                }
                return textColor;
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; NotifyPropertyChanged(); }
        }

        public LevelsVM(LevelTypes level) => LevelType = level;
    }
}
