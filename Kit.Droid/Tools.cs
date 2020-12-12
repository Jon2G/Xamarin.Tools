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
using SQLHelper;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Log = SQLHelper.Log;
using Application = Android.App.Application;
using Kit.Droid.Services;

namespace Kit.Droid
{
    public class Tools : Kit.Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init(MainActivity activity, Bundle bundle)
        {
            Xamarin.Essentials.Platform.Init(activity, bundle);
            Xamarin.Forms.Forms.Init(activity, bundle);
            FormsMaterial.Init(activity, bundle);

            Acr.UserDialogs.UserDialogs.Init(activity);
            CrossCurrentActivity.Current.Init(activity, bundle);
            Rg.Plugins.Popup.Popup.Init(activity);

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;
            Set(new ToolsImplementation());
            Instance.SetDebugging(Debugger.IsAttached);
            (Instance as ToolsImplementation).MainActivity = activity;
            CrossCurrentActivity.Current.Init(activity, bundle);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            OrientationServices(activity);
            CrossFingerprint.SetCurrentActivityResolver(() => CrossCurrentActivity.Current.Activity);
            return Instance;
        }
        public static AbstractTools InitLoaded(Application app, Activity activity, Bundle bundle)
        {
            return Kit.Tools.Instance;
        }
        private static void OrientationServices(Activity activity)
        {
            MessagingCenter.Subscribe<Page>(activity, nameof(DeviceOrientation.Landscape), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Landscape;
            });
            MessagingCenter.Subscribe<Page>(activity, nameof(DeviceOrientation.Portrait), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Portrait;
            });
            MessagingCenter.Subscribe<Page>(activity, nameof(DeviceOrientation.Other), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Unspecified;
            });
        }
    }
}
