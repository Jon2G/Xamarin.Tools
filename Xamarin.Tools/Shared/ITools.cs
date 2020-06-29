using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared
{
    public interface ITools
    {
        ITools InitAll(string LogDirectory, bool AlertAfterCritical = false);
        ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false);
        void CriticalAlert(object sender, EventArgs e);
    }
}
