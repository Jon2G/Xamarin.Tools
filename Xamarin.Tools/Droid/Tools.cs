using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Xamarin.Tools.Droid.Services;
using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Logging;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace Plugin.Xamarin.Tools.Droid
{
    public partial class Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init(Activity activity, Bundle bundle)
        {
            global::Xamarin.Essentials.Platform.Init(activity, bundle);
            global::Xamarin.Forms.Forms.Init(activity, bundle);
            global::Xamarin.Forms.FormsMaterial.Init(activity, bundle);

            Acr.UserDialogs.UserDialogs.Init(activity);
            CrossCurrentActivity.Current.Init(activity, bundle);
            Rg.Plugins.Popup.Popup.Init(activity, bundle);

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Instance.SetDebugging(Debugger.IsAttached);
            (Instance as ToolsImplementation).MainActivity = activity;
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(activity, bundle);
            #region DependencyServices
            DependencyService.Register<DataShare>();
            DependencyService.Register<PDFSaveAndOpen>();
            DependencyService.Register<PhotoPickerService>();
            DependencyService.Register<PrintHTML>();
            DependencyService.Register<Screenshot>();
            #endregion

            return Instance;
        }
        public static AbstractTools InitLoaded(Application app, Activity activity, Bundle bundle)
        {
            // IMPORTANT: Initialize XFGloss AFTER calling LoadApplication on the Android platform
            XFGloss.Droid.Library.Init(activity, bundle);
            return Instance;
        }

        static AbstractTools currentInstance;
        public static AbstractTools Instance
        {
            get
            {
                if (currentInstance == null)
                    throw new ArgumentException("[Shared.Tools] In android, you must call UserDialogs.Init(Activity) from your first activity OR UserDialogs.Init(App) from your custom application OR provide a factory function to get the current top activity via UserDialogs.Init(() => supply top activity)");

                return currentInstance;
            }
            set => currentInstance = value;
        }
    }
}
