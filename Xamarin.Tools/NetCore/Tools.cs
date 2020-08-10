using Plugin.Xamarin.Tools.NetCore;
using Plugin.Xamarin.Tools.Shared;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Xamarin.Tools.NetCore
{
    public static partial class Tools
    {
        public static void Set(AbstractTools Instance)
        {
            currentInstance = Instance;
        }

        static AbstractTools currentInstance;
        public static AbstractTools Instance
        {
            get
            {
                if (currentInstance == null)
                    throw new ArgumentException("[Shared.Tools] This is the bait library, not the platform library.  You must install the nuget package in your main executable/application project");

                return currentInstance;
            }
            set => currentInstance = value;
        }
        public static AbstractTools Init()
        {

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Shared.Tools.Set(new ToolsImplementation());
            Shared.Tools.Instance.SetDebugging(Debugger.IsAttached);


            return Shared.Tools.Instance;
        }
    }
}
