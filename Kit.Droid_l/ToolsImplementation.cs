using System;
using Kit.Enums;
using Kit.Droid.Services;
using Serilog;
using Kit.Forms.Services;

namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public override string TemporalPath => Xamarin.Essentials.FileSystem.CacheDirectory;
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.Android;
        public MainActivity MainActivity { get; private set; }

        public new static ToolsImplementation Instance => Kit.Tools.Instance as ToolsImplementation;

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
                //Log to my sink logger
                .WriteTo.Async(x => x.Sink(Log.LogsSink))
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