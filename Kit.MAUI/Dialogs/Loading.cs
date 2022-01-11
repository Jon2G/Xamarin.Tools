using Kit.Dialogs;
using System;

namespace Kit.MAUI.Dialogs
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
            return Acr.UserDialogs.UserDialogs.Instance.Loading(Text);
        }
    }
}