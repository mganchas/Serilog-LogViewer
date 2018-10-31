﻿using System;
using System.Windows.Input;

namespace LogViewer.Services
{
    public class CommandHandler : ICommand
    {
        private readonly Action<object> _action;
        private readonly bool _canExecute;

        public CommandHandler(Action<object> action)
        {
            this._action = action;
            this._canExecute = true;
        }

        public CommandHandler(Action<object> action, bool canExecute)
        {
            this._action = action;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
