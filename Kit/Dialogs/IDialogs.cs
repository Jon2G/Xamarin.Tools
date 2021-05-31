using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Dialogs
{
    public interface IDialogs
    {
        ILoading Loading { get; }
        ICustomMessageBox CustomMessageBox { get; }
    }
}