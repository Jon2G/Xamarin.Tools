using Foundation;
using Plugin.Xamarin.Tools.iOS.Services;
using Plugin.Xamarin.Tools.Shared;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Log = SQLHelper.Log;

namespace Plugin.Xamarin.Tools.iOS
{
    public partial class Tools
    {
        public static AbstractTools Init()
        {
            //////////////////////////////////////////
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.FormsMaterial.Init();

            Rg.Plugins.Popup.Popup.Init();

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;
            Shared.Tools.Set(new ToolsImplementation());
            Shared.Tools.Instance.SetDebugging(Debugger.IsAttached);
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                        (approved, error) => { });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            return Shared.Tools.Instance;
        }
    }
}
