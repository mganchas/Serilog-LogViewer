using System;
using System.Windows.Input;
using System.Windows.Media;
using static LiveViewer.Utils.Levels;

namespace LiveViewer.ViewModel
{
    public class LevelsVM : BaseVM
    {
        public ICommand ClickCommand { get; set; }

        public string Text { get { return Enum.GetName(typeof(LevelTypes), LevelType); } }
        public LevelTypes LevelType { get; set; }

        private int counter;
        public int Counter
        {
            get { return counter; }
            set { counter = value; NotifyPropertyChanged(); }
        }

        private Brush textColor;
        public Brush TextColor
        {
            get { return textColor; }
            set { textColor = value; NotifyPropertyChanged(); }
        }
    }
}
