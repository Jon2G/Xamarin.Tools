using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.iOS
{
    public class ToolsImplementation : AbstractTools
    {

        public override ITools InitAll(string LogDirectory, bool AlertAfterCritical = false)
        {
           return InitLoggin(LogDirectory, AlertAfterCritical);
        }

        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
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

        public override void SetDebugging(bool Debugging)
        {
            Debugging = true;
        }
        public override void CriticalAlert(object sender, EventArgs e)
        {
            DependencyService.Get<ICustomMessageBox>()
                .ShowOK(sender.ToString(), "Alerta", "Entiendo", Shared.Enums.CustomMessageBoxImage.Error);
        }
    }
}
