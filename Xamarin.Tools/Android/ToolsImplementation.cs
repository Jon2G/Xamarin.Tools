using System;
using System.Collections.Generic;
using System.Text;
using Android.Util;
using Plugin.Xamarin.Tools.Shared;

namespace Plugin.Xamarin.Tools.Android
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
