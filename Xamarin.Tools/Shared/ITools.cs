using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared
{
    public interface ITools
    {
        ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false);
    }
}
