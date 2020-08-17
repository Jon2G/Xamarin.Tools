using Plugin.Xamarin.Tools.Shared;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Xamarin.Tools.NetCore
{
    public class ToolsImplementation : AbstractTools
    {
        public override ITools InitAll(string LogDirectory, bool AlertAfterCritical = false)
        {
            Debugging = Debugger.IsAttached;
            return InitLoggin(LogDirectory, AlertAfterCritical);
        }

        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
            if (AlertAfterCritical)
            {
                Log.Init(LogDirectory, CriticalAlert);
            }
            else
            {
                Log.Init(LogDirectory);
            }
            return this;
        }

        public override AbstractTools SetDebugging(bool Debugging)
        {
            this.Debugging = Debugging;
            SQLHelper.SQLHelper.Instance?.SetDebugging(Debugging);
            return this;
        }
        public override void CriticalAlert(object sender, EventArgs e)
        {
            //DependencyService.Get<ICustomMessageBox>()
            //    .ShowOK(sender.ToString(), "Alerta", "Entiendo", Shared.Enums.CustomMessageBoxImage.Error);
        }

    }
}
