using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Xamarin.Tools.Shared;

namespace Plugin.Xamarin.Tools.UWP
{
    /// <summary>
    /// Interface for Xamarin.Tools
    /// </summary>
    public class XamarinImplementation : Plugin.Xamarin.Tools.Shared.AbstractTools
    {
        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
            throw new NotImplementedException();
        }
    }
}
