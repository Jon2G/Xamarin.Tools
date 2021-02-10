using System;

namespace Kit.Extensions
{
    public class TouchControlCommand : CommonCommand
    {
        public TouchControlCommand(Action<object> ExecuteAction) : base(ExecuteAction) { }
        public TouchControlCommand(Func<bool> CanExecuteFunction, Action<object> ExecuteAction) : base(CanExecuteFunction, ExecuteAction) { }
        protected override bool CanBeExecuted => ClickHelper.EsValido();

    }
}
