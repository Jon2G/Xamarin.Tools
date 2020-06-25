using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared
{
    public abstract class AbstractTools : ITools
    {

        public AbstractTools()
        {

        }

        public abstract ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false);
    }
}
