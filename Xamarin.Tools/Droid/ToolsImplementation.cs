using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Android.App;
using Android.Util;
using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

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
            DependencyService.Get<ICustomMessageBox>()
                .ShowOK(sender.ToString(), "Alerta", "Entiendo", Shared.Enums.CustomMessageBoxImage.Error);
        }

        public override void SetDebugging(bool Debugging)
        {
            Debugging = true;
        }
    }
}
