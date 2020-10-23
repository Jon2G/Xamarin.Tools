using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using SQLHelper;
using Tools.Data;
using Xamarin.Forms;


namespace Tools.UWP
{
    /// <summary>
    /// Interface for Xamarin.Tools
    /// </summary>
    public partial class Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init(string LibraryPath)
        {
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Data.Tools.Set(new ToolsImplementation());
            Data.Tools.Instance.SetDebugging(Debugger.IsAttached);
            Data.Tools.Instance.SetLibraryPath(LibraryPath);


            //     ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();


            #region DependencyServices
            //DependencyService.Register<DataShare>();
            //DependencyService.Register<PDFSaveAndOpen>();
            //DependencyService.Register<PhotoPickerService>();
            //DependencyService.Register<PrintHTML>();
            //DependencyService.Register<Services.DeviceInfo>();
            // DependencyService.Register<Screenshot>();
            //DependencyService.Register<CustomMessageBoxService>();
            #endregion
            Data.Tools.Instance.SetDebugging(Debugger.IsAttached);
            return Data.Tools.Instance;
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
