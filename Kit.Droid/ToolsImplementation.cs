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
using Kit.Droid.Services;
using Kit.Services.Interfaces;

namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public MainActivity MainActivity { get; internal set; }
        public override ITools Init(string LogDirectory = "Logs", bool AlertAfterCritical = false)
        {
            this.CustomMessageBox = new Services.CustomMessageBoxService();
            Debugging = Debugger.IsAttached;
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
