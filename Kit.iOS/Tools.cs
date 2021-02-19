using Foundation;
using Kit.iOS.Services;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using FFImageLoading.Forms.Platform;
using Log = Kit.Sql.Log;

namespace Kit.iOS
{
    public class Tools : Kit.Tools
    {
        public static AbstractTools Init()
        {
            //////////////////////////////////////////
            Xamarin.Forms.Forms.Init();
            FormsMaterial.Init();

            Rg.Plugins.Popup.Popup.Init();
            CachedImageRenderer.Init();
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;
            Set(new ToolsImplementation());
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            TouchEffect.iOS.TouchEffectPreserver.Preserve();
            TouchEffect.iOS.PlatformTouchEff.Preserve();

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
                UIUserNotificationSettings settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            return Kit.Tools.Instance;
        }

    }
}
