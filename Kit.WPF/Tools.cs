using System;
using System.Threading.Tasks;

namespace Kit.WPF
{
    /// <summary>
    /// Interface for Xamarin.Tools
    /// </summary>
    public class Tools : Kit.Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init(string LibraryPath = null)
        {
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;
            Kit.Tools.Set(new ToolsImplementation(LibraryPath));
            (Kit.Tools.Instance as ToolsImplementation).Init();
            // ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();
            Kit.Tools.Instance.Init();
            return Kit.Tools.Instance;
        }

        public static new ToolsImplementation Instance
        {
            get => (ToolsImplementation)Kit.Tools.Instance;
        }

        //static AbstractTools currentInstance;
        //public static AbstractTools Instance
        //{
        //    get
        //    {
        //        if (currentInstance == null)
        //            throw new ArgumentException("[Shared.Tools] In android, you must call UserDialogs.Init(Activity) from your first activity OR UserDialogs.Init(App) from your custom application OR provide a factory function to get the current top activity via UserDialogs.Init(() => supply top activity)");

        //        return currentInstance;
        //    }
        //    set => currentInstance = value;
        //}
    }
}