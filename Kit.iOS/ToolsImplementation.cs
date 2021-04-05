using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kit;
using Kit.Forms.Services;
using Kit.iOS.Services;
using Kit.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Kit.Services.Interfaces;
using Serilog;
using DeviceInfo = Kit.iOS.Services.DeviceInfo;
using Kit.Enums;
using Kit.Forms.Controls.CrossImage;

namespace Kit.iOS
{
    public class ToolsImplementation : AbstractTools
    {
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.iOS;
        public override void Init()
        {
            Init(new DeviceInfo(), new CustomMessageBoxService(), new SynchronizeInvoke(), new ScreenManagerService(), new ImageExtensions(), new BarCodeBuilder());
            Log.Init().SetLogger((new LoggerConfiguration()
                // Set default log level limit to Debug
                .MinimumLevel.Debug()
                // Enrich each log entry with memory usage and thread ID
                // .Enrich.WithMemoryUsage()
                //.Enrich.WithThreadId()
                // Write entries to ios log (Nuget package Serilog.Sinks.Xamarin)
                .WriteTo.NSLog()
                // Create a custom logger in order to set another limit,
                // particularly, any logs from Information level will also be written into a rolling file
                .WriteTo.Logger(config =>
                    config
                        .MinimumLevel.Information()
                        .WriteTo.File(Log.Current.LoggerPath, retainedFileCountLimit: 7)
                )
                // And create another logger so that logs at Fatal level will immediately send email
                .WriteTo.Logger(config =>
                    config
                        .MinimumLevel.Fatal()
                        .WriteTo.File(Log.Current.CriticalLoggerPath, retainedFileCountLimit: 1)
                )).CreateLogger(), CriticalAlert);
        }



        public override void CriticalAlert(object sender, EventArgs e)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(sender.ToString(), "Alerta", "Entiendo");
        }
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(Page mainPage)
        {
            if (mainPage.Navigation.NavigationStack.Any() && mainPage.Navigation.NavigationStack.Last() is Page page)
            {
                if (page.GetType().GetProperty("LockedOrientation") is System.Reflection.PropertyInfo LockedOrientationProperty)
                {
                    if (LockedOrientationProperty.GetValue(page) is DeviceOrientation LockedOrientation)
                    {
                        if (LockedOrientation != DeviceOrientation.Other)
                        {
                            page.Disappearing += Page_Disappearing;
                            switch (LockedOrientation)
                            {
                                case DeviceOrientation.Landscape:
                                    return UIInterfaceOrientationMask.Landscape;
                                case DeviceOrientation.Portrait:
                                    return UIInterfaceOrientationMask.Portrait;

                            }
                        }
                    }
                }
            }
            return UIInterfaceOrientationMask.All;
        }
        private void Page_Disappearing(object sender, EventArgs e)
        {
            (sender as Page).Disappearing -= Page_Disappearing;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)UIInterfaceOrientation.Unknown), new NSString("orientation"));
        }
    }
}
