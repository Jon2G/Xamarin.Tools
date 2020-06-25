using Plugin.Xamarin.Tools.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.iOS
{
    public class ToolsImplementation : AbstractTools
    {
        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
            Shared.Logging.Log.Init(LogDirectory, AlertAfterCritical);
            return this;
        }
    }
}
