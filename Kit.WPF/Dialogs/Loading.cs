using Kit.Dialogs;
using System;

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