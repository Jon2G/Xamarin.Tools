using Foundation;
using Kit;
using Kit.Dialogs;
using Kit.Enums;
using Kit.Forms.Services;
using Kit.iOS.Services;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using Serilog;
using System;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
namespace Kit.iOS
{
    public class ToolsImplementation : AbstractTools
    {
        public override string TemporalPath => Xamarin.Essentials.FileSystem.CacheDirectory;
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.iOS;
        public override AbstractTools Init()
        {
            Kit.Tools.Container.Register<ISynchronizeInvoke, SynchronizeInvoke>();
            Kit.Tools.Container.Register<IDialogs, Kit.Forms.Dialogs.Dialogs>();
            Kit.Tools.Container.Register<IScreenManager, ScreenManagerService>();
            Kit.Tools.Container.Register<Kit.Controls.CrossImage.CrossImageExtensions, Kit.Forms.Controls.CrossImage.CrossImageExtensions>();
            Kit.Tools.Container.Register<IBarCodeBuilder, BarCodeBuilder>();
            Kit.Tools.Container.Register<IClipboardService, ClipboardService>();
            Kit.Tools.Container.Register<Plugin.DeviceInfo.Abstractions.IDeviceInfo, Plugin.DeviceInfo.DeviceInfoImplementation>();
            Log.Init((l) =>
            {
                return (new LoggerConfiguration()
                     // Set default log level limit to Debug
                     .MinimumLevel.Debug()
                     // Enrich each log entry with memory usage and thread ID
                     // .Enrich.WithMemoryUsage()
                     //.Enrich.WithThreadId()
                     // Write entries to ios log (Nuget package Serilog.Sinks.Xamarin)
                     .WriteTo.NSLog()
                     // Create a custom logger in order to set another limit,
                     // particularly, any logs from Information level will also be written into a rolling file

                     .WriteTo.Async(x => x.Sink(Kit.Log.LogsSink))
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
                     )).CreateLogger();
            });
            return this;
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
