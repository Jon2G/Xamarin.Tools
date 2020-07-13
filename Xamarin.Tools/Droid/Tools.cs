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
using SQLHelper;
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
            Shared.Tools.Set(new ToolsImplementation());
            Shared.Tools.Instance.SetDebugging(Debugger.IsAttached);
            (Shared.Tools.Instance as ToolsImplementation).MainActivity = activity;
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(activity, bundle);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
    
            #region DependencyServices
            DependencyService.Register<DataShare>();
            DependencyService.Register<PDFSaveAndOpen>();
            DependencyService.Register<PhotoPickerService>();
            DependencyService.Register<PrintHTML>();
            DependencyService.Register<Screenshot>();
            DependencyService.Register<CustomMessageBoxService>();
            DependencyService.Register<Services.DeviceInfo>();
            #endregion

            return Shared.Tools.Instance;
        }
        public static AbstractTools InitLoaded(Application app, Activity activity, Bundle bundle)
        {
            // IMPORTANT: Initialize XFGloss AFTER calling LoadApplication on the Android platform
            XFGloss.Droid.Library.Init(activity, bundle);
            return Shared.Tools.Instance;
        }
    }
}
