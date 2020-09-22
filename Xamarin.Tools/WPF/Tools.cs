using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.WPF.Services;
using Plugin.Xamarin.Tools.WPF.Services.ICustomMessageBox;
using SQLHelper;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Log = SQLHelper.Log;

namespace Plugin.Xamarin.Tools.WPF
{
    /// <summary>
    /// Interface for Xamarin.Tools
    /// </summary>
    public partial class Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init()
        {

            global::Xamarin.Forms.Forms.Init();

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Shared.Tools.Set(new ToolsImplementation());
            Shared.Tools.Instance.SetDebugging(Debugger.IsAttached);

            // ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();

            #region DependencyServices
            //DependencyService.Register<DataShare>();
            //DependencyService.Register<PDFSaveAndOpen>();
            //DependencyService.Register<PhotoPickerService>();
            //DependencyService.Register<PrintHTML>();
            //DependencyService.Register<Services.DeviceInfo>();
            //// DependencyService.Register<Screenshot>();
            //DependencyService.Register<CustomMessageBoxService>();
            #endregion

            return Shared.Tools.Instance;
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
