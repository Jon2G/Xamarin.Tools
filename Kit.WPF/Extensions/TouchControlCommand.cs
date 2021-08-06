using System;
using Kit;
using Kit.Extensions;

namespace Kit
{
    public class TouchControlCommand : TouchControlCommand<object>
    {
        public TouchControlCommand(Action<object> ExecuteAction) : base(ExecuteAction)
        {
        }

        public TouchControlCommand(Func<object, bool> CanExecuteFunction, Action<object> ExecuteAction) : base(CanExecuteFunction, ExecuteAction)
        {
        }
    }

    public class TouchControlCommand<T> : Command<T>
    {
        public TouchControlCommand(Action<T> ExecuteAction) : base(ExecuteAction)
        {
        }

        public TouchControlCommand(Func<T, bool> CanExecuteFunction, Action<T> ExecuteAction) : base(ExecuteAction, CanExecuteFunction)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return ClickHelper.EsValido() && base.CanExecute(parameter);
        }
    }
}