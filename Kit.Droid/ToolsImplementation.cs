using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Android.App;
using Android.Util;
using Android.Views;
using Kit.Enums;
using Xamarin.Essentials;
using Xamarin.Forms;
using Kit.Services;
using Kit.Droid.Services;
using Kit.Services.Interfaces;
using Serilog;
using Android.Telephony;
using Kit.Forms.Controls.CrossImage;
using Kit.Forms.Services;
using Kit.Forms.Extensions;

namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.Android;
        public MainActivity MainActivity { get; private set; }
        public static Kit.Droid.ToolsImplementation Instance => Tools.Instance as Kit.Droid.ToolsImplementation;

        public void Init(MainActivity MainActivity)
        {
            this.MainActivity = MainActivity;
            Init();
        }

        public override void Init()
        {
            Init(new Kit.Forms.Dialogs.Dialogs(),
                new SynchronizeInvoke(), new ScreenManagerService(),
                new Kit.Forms.Controls.CrossImage.CrossImageExtensions(), new BarCodeBuilder());

            Log.Init().SetLogger((new LoggerConfiguration()
                // Set default log level limit to Debug
                .MinimumLevel.Debug()
                // Enrich each log entry with memory usage and thread ID
                // .Enrich.WithMemoryUsage()
                //.Enrich.WithThreadId()
                // Write entries to Android log (Nuget package Serilog.Sinks.Xamarin)
                .WriteTo.Async(x => x.AndroidLog())
                // Create a custom logger in order to set another limit,
                // particularly, any logs from Information level will also be written into a rolling file
                .WriteTo.Async(x => x.Logger(config =>
                      config
                          .MinimumLevel.Information()
                          .WriteTo.Async(x => x.File(
                              path: Log.Current.LoggerPath,
                              retainedFileCountLimit: 7,
                              flushToDiskInterval: TimeSpan.FromMilliseconds(500)))
                ))
                // And create another logger so that logs at Fatal level will immediately send email
                .WriteTo.Logger(config =>
                    config
                        .MinimumLevel.Fatal()
                        .WriteTo.Async(x => x.File(
                            path: Log.Current.CriticalLoggerPath,
                            retainedFileCountLimit: 1,
                             flushToDiskInterval: TimeSpan.FromMilliseconds(500)))
                )).CreateLogger(), CriticalAlert);
        }

        public override void CriticalAlert(object sender, EventArgs e)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(sender.ToString(), "Alerta", "Entiendo");
        }
    }
}