using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Kit.NetCore
{
    public class Tools : Kit.Tools
    {
        public static AbstractTools Init()
        {

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;


            Instance = new ToolsImplementation();
            Instance.SetDebugging(Debugger.IsAttached);


            return Instance;
        }
    }
}
