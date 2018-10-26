using System;
using System.Windows.Input;

namespace LogViewer.Services
{
    public class CommandHandler : ICommand
    {
        private Action<object> action;
        private bool canExecute;

        public CommandHandler(Action<object> action)
        {
            this.action = action;
            this.canExecute = true;
        }

        public CommandHandler(Action<object> action, bool canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
