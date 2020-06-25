using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared
{
    public static partial class Tools
    {

#if NETSTANDARD
        static ITools currentInstance;
        public static ITools Instance
        {
            get
            {
                if (currentInstance == null)
                    throw new ArgumentException("[Shared.Tools] This is the bait library, not the platform library.  You must install the nuget package in your main executable/application project");

                return currentInstance;
            }
            set => currentInstance = value;
        }
#endif

        public static bool Debugging { get; private set; }
        public static void Init()
        {
            Debugging = Debugger.IsAttached;

        }
        public static void InitAll(string LogDirectory)
        {
            Logging.Log.Init(LogDirectory);
        }
        public static void SetDebugging(bool Debugging)
        {
            Tools.Debugging = true;
        }

    }
}
