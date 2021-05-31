using Kit.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.WPF.Dialogs
{
    internal class Loading : ILoading
    {
        public Loading()
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDisposable Show(string Text = "Cargando...")
        {
            throw new NotImplementedException();
        }
    }
}