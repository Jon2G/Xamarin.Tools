using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Kit.Extensions
{
    public class CommonCommand : ICommand
    {
        private bool _CanExecute;
        public bool CanExecute
        {
            get
            {
                bool can;
                if (this.CanExecuteFunction is null)
                {
                    can = true;
                }
                else
                {
                    can = CanExecuteFunction.Invoke();
                }
                if (can != _CanExecute)
                {
                    _CanExecute = can;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
                return _CanExecute;
            }
        }
        public event EventHandler CanExecuteChanged;
        private Func<bool> CanExecuteFunction;
        private Action<object> ExecuteAction;
        public CommonCommand(Func<bool> CanExecuteFunction, Action<object> ExecuteAction)
        {
            this.CanExecuteFunction = CanExecuteFunction;
            this.ExecuteAction = ExecuteAction;
        }

        public CommonCommand(Action<object> ExecuteAction)
        {
            this.CanExecuteFunction = null;
            this.ExecuteAction = ExecuteAction;
        }

        public void Execute(object parameter)
        {
            if (CanBeExecuted)
                ExecuteAction.Invoke(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute;
        }

        protected virtual bool CanBeExecuted
        {
            get => true;
        }
    }
}
