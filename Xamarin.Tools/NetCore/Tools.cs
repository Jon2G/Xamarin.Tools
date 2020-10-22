using Plugin.Xamarin.Tools.NetCore;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Data= Tools.Data;

namespace Tools.NetCore
{
    public static partial class Tools
    {
        public static Data.AbstractTools Init()
        {

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;


            Data.Tools.Instance = new ToolsImplementation();
            Data.Tools.Instance.SetDebugging(Debugger.IsAttached);


            return Data.Tools.Instance;
        }
    }
}
