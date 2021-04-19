using System;
using Kit.Extensions;

namespace Kit.WPF.Extensions
{
    public class TouchControlCommand : CommonCommand<object>
    {
        public TouchControlCommand(Action<object> ExecuteAction) : base(ExecuteAction) { }
        public TouchControlCommand(Func<bool> CanExecuteFunction, Action<object> ExecuteAction) : base(CanExecuteFunction, ExecuteAction) { }
        protected override bool CanBeExecuted => ClickHelper.EsValido();

    }
}
