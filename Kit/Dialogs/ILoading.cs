using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Dialogs
{
    public interface ILoading : IDisposable
    {
        IDisposable Show(string Text = "Cargando...");
    }
}