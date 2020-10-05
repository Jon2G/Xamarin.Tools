using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Plugin.Xamarin.Tools.Shared.Classes
{
    public class CommonCommand : ICommand
    {
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
        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteFunction is null)
            {
                return true;
            }
            return CanExecuteFunction.Invoke();
        }

        public void Execute(object parameter)
        {
            ExecuteAction.Invoke(parameter);
        }
    }
}
