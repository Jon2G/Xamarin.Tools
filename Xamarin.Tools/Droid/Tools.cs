using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Fingerprint;
using Plugin.Xamarin.Tools.Droid.Services;
using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Pages;
using SQLHelper;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Application = Android.App.Application;
using Log = SQLHelper.Log;

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
            OrientationServices(activity);
            CrossFingerprint.SetCurrentActivityResolver(() => CrossCurrentActivity.Current.Activity);
            return Shared.Tools.Instance;
        }
        public static AbstractTools InitLoaded(Application app, Activity activity, Bundle bundle)
        {
            return Shared.Tools.Instance;
        }
        private static void OrientationServices(Activity activity)
        {
            MessagingCenter.Subscribe<BasePage>(activity, nameof(DeviceOrientation.Landscape), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Landscape;
            });
            MessagingCenter.Subscribe<BasePage>(activity, nameof(DeviceOrientation.Portrait), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Portrait;
            });
            MessagingCenter.Subscribe<BasePage>(activity, nameof(DeviceOrientation.Other), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Unspecified;
            });
        }
    }
}
