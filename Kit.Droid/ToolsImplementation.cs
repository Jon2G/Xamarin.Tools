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

namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public MainActivity MainActivity { get; internal set; }
        public override ITools Init(IDeviceInfo DeviceInfo)
        {
            this.CustomMessageBox = new Services.CustomMessageBoxService();
            base.Init(DeviceInfo);

            Log.Init().SetLogger((new LoggerConfiguration()
                // Set default log level limit to Debug
                .MinimumLevel.Debug()
                // Enrich each log entry with memory usage and thread ID
                // .Enrich.WithMemoryUsage()
                //.Enrich.WithThreadId()
                // Write entries to Android log (Nuget package Serilog.Sinks.Xamarin)
                .WriteTo.AndroidLog()
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

            return this;
        }

        public override void CriticalAlert(object sender, EventArgs e)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(sender.ToString(), "Alerta", "Entiendo");
        }


    }
}
