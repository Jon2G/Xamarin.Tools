using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Android.App;
using Android.Util;
using Android.Views;
using Kit.Enums;
using Xamarin.Essentials;
using Xamarin.Forms;
using Kit.Services;

namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public Activity MainActivity { get; internal set; }
        public override ITools InitAll(string LogDirectory, bool AlertAfterCritical = false)
        {
            Debugging = Debugger.IsAttached;
            return InitLoggin(LogDirectory, AlertAfterCritical);
        }

        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical)
        {
            if (AlertAfterCritical)
            {
                SQLHelper.Log.Init(LogDirectory, CriticalAlert);
            }
            else
            {
                SQLHelper.Log.Init(LogDirectory);
            }

            return this;
        }

        public override void CriticalAlert(object sender, EventArgs e)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(sender.ToString(), "Alerta", "Entiendo");
        }

        public override AbstractTools SetDebugging(bool Debugging)
        {
            this.Debugging = true;
            return this;
        }
    }
}
