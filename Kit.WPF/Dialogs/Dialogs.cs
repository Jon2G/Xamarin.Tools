using Kit.Dialogs;
using Kit.WPF.Dialogs.ICustomMessageBox;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.WPF.Dialogs
{
    public class Dialogs : IDialogs
    {
        public ILoading Loading => new Loading();

        private Kit.Dialogs.ICustomMessageBox _CustomMessageBox;
        public Kit.Dialogs.ICustomMessageBox CustomMessageBox => _CustomMessageBox ??= new CustomMessageBoxService();
    }
}