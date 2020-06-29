using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Android.App;
using Android.Util;
using Plugin.Xamarin.Tools.Shared;

namespace Plugin.Xamarin.Tools.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public Activity MainActivity { get; internal set; }
        public override ITools InitAll(string LogDirectory, bool AlertAfterCritical = false)
        {
            Debugging = Debugger.IsAttached;
            return InitLoggin(LogDirectory, AlertAfterCritical);
        }

        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
            SQLHelper.Log.AlertCritical += CriticalAlert;
            SQLHelper.Log.Init(LogDirectory, AlertAfterCritical);
            return this;
        }

        public override void SetDebugging(bool Debugging)
        {
            Debugging = true;
        }

        public override async void CriticalAlert(object sender, EventArgs e)
        {
           await  Acr.UserDialogs.UserDialogs.Instance.AlertAsync(sender.ToString(), "Alerta", "Entiendo");
        }
    }
}
