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
        public static AbstractTools Init()
        {

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Shared.Tools.Instance = new ToolsImplementation();
            Shared.Tools.Instance.SetDebugging(Debugger.IsAttached);


            return Shared.Tools.Instance;
        }
    }
}
