using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using System.Threading;
using Process = System.Diagnostics.Process;
using Thread = System.Threading.Thread;
namespace Kit.WPF.Services
{
    public class AtachDebugger
    {
        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        private static IEnumerable<DTE> GetInstances()
        {
            IRunningObjectTable rot;
            IEnumMoniker enumMoniker;
            int retVal = GetRunningObjectTable(0, out rot);

            if (retVal == 0)
            {
                rot.EnumRunning(out enumMoniker);

                IntPtr fetched = IntPtr.Zero;
                IMoniker[] moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);
                    Console.WriteLine("Display Name: {0}", displayName);
                    bool isVisualStudio = displayName.StartsWith("!VisualStudio");
                    if (isVisualStudio)
                    {
                        object obj;
                        rot.GetObject(moniker[0], out obj);
                        var dte = obj as DTE;
                        yield return dte;
                    }
                }
            }
        }





        private static EnvDTE.DTE LaunchVsDte(bool isPreRelease)
        {
            Log.Logger.Debug("Starting Vs2019");
            System.Type type = Type.GetTypeFromProgID("VisualStudio.DTE.16.0");
            EnvDTE.DTE dte = (EnvDTE.DTE)System.Activator.CreateInstance(type);
            Log.Logger.Debug("Instance successfully created");
            dte.MainWindow.Visible = true;
            return dte;
        }
        //    ISetupInstance setupInstance = GetSetupInstance(isPreRelease);
        //    string installationPath = setupInstance.GetInstallationPath();
        //    string executablePath = Path.Combine(installationPath, @"Common7\IDE\devenv.exe");
        //    Process vsProcess = Process.Start(executablePath);
        //    string runningObjectDisplayName = $"VisualStudio.DTE.16.0:{vsProcess.Id}";

        //    IEnumerable<string> runningObjectDisplayNames = null;
        //    object runningObject;
        //    for (int i = 0; i < 60; i++)
        //    {
        //        try
        //        {
        //            runningObject = GetRunningObject(runningObjectDisplayName, out runningObjectDisplayNames);
        //        }
        //        catch
        //        {
        //            runningObject = null;
        //        }

        //        if (runningObject != null)
        //        {
        //            return (EnvDTE.DTE)runningObject;
        //        }

        //        Thread.Sleep(millisecondsTimeout: 1000);
        //    }

        //    throw new TimeoutException($"Failed to retrieve DTE object. Current running objects: {string.Join(";", runningObjectDisplayNames)}");
        //}

        public static bool AttachToCurrent()
        {
            try
            {
                List<DTE> instances = GetInstances().ToList();
                if (instances.Any())
                return    Attach(instances.First());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "AttachToCurrent");
            }
            return false;
        }
        public static void Attach()
        {
            Attach(LaunchVsDte(false));
        }
        public static bool Attach(DTE dte)
        {
            string myprocesss = Process.GetCurrentProcess().ProcessName;
            var processes = dte.Debugger.LocalProcesses;
            foreach (var proc in processes.Cast<EnvDTE.Process>())
            {
                if (proc.Name.Contains(myprocesss))
                {
                    proc.Attach();
                    return true;
                }
            }
            return false;

        }
    }
}
