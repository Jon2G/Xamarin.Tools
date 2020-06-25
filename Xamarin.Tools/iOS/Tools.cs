using Plugin.Xamarin.Tools.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.iOS
{
    public partial class Tools
    { 
        static ITools currentInstance;
        public static ITools Instance
        {
            get
            {
                currentInstance = currentInstance ?? new ToolsImplementation();
                return currentInstance;
            }
            set => currentInstance = value;
        }
    }
}
