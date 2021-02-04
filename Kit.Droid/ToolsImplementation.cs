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
using Log = Kit.Sql.Log;
namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public MainActivity MainActivity { get; internal set; }
        public override ITools Init(IDeviceInfo DeviceInfo, string LogDirectory = "Logs", bool AlertAfterCritical = false)
        {
            this.CustomMessageBox = new Services.CustomMessageBoxService();
            base.Init(DeviceInfo, LogDirectory, AlertAfterCritical);
            Debugging = Debugger.IsAttached;
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
