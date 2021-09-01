using Kit.Dialogs;
using Kit.WPF.Dialogs.ICustomMessageBox;
using System;
using System.Threading.Tasks;

namespace Kit.WPF.Dialogs
{
    public class Dialogs : IDialogs
    {
        public ILoading Loading => new Loading();

        private Kit.Dialogs.ICustomMessageBox _CustomMessageBox;
        public Kit.Dialogs.ICustomMessageBox CustomMessageBox => _CustomMessageBox ??= new CustomMessageBoxService();

        public Task<LoginResult> LoginAsync(string title = null, string message = null)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResult> LoginAsync(LoginConfig config)
        {
            throw new NotImplementedException();
        }
    }
}