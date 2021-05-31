using Kit.Dialogs;
using Kit.Forms.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(Dialogs))]

namespace Kit.Forms.Dialogs
{
    public class Dialogs : IDialogs
    {
        public ILoading Loading => new Loading();

        private ICustomMessageBox _CustomMessageBox;
        public ICustomMessageBox CustomMessageBox => _CustomMessageBox ??= new CustomMessageBox();
    }
}