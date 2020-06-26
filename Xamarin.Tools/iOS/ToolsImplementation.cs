using Plugin.Xamarin.Tools.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
            Shared.Logging.Log.Init(LogDirectory, AlertAfterCritical);
            return this;
        }

        public override void SetDebugging(bool Debugging)
        {
            Debugging = true;
        }
    }
}
