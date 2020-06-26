﻿using Foundation;
using Plugin.Xamarin.Tools.iOS.Services;
using Plugin.Xamarin.Tools.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.iOS
{
    public partial class Tools
    {
        public static AbstractTools Init()
        {
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.FormsMaterial.Init();
            /********** ADD THIS CALL TO INITIALIZE XFGloss *********/
            XFGloss.iOS.Library.Init();
            //FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            //CrossMedia.Current.Initialize();
            Rg.Plugins.Popup.Popup.Init();

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

            #region DependencyServices
            DependencyService.Register<DataShare>();
            DependencyService.Register<PDFSaveAndOpen>();
            DependencyService.Register<PhotoPickerService>();
            DependencyService.Register<PrintHTML>();
            DependencyService.Register<Screenshot>();
            #endregion

            return Instance;
        }

        static AbstractTools currentInstance;
        public static AbstractTools Instance
        {
            get
            {
                currentInstance = currentInstance ?? new ToolsImplementation();
                return currentInstance;
            }
            set => currentInstance = value;
        }
    }
}
