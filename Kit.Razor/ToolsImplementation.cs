using Kit.Enums;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kit.Razor
{
    public class ToolsImplementation : AbstractTools
    {
        public ToolsImplementation(string? LibraryPath = null)
        {
            this._LibraryPath = LibraryPath;
        }

        public string ProductName()
        {
            return Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute))
                .OfType<AssemblyProductAttribute>()
                .FirstOrDefault().Product;
        }

        private string _LibraryPath;
        public override string LibraryPath => _LibraryPath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.WPF;
        public static new Kit.Razor.ToolsImplementation Instance => Tools.Instance as Kit.Razor.ToolsImplementation;

        public override AbstractTools Init()
        {
            //Kit.Tools.Container.Register<ISynchronizeInvoke, SynchronizeInvoke>();
            //Kit.Tools.Container.Register<IDialogs, Kit.WPF.Dialogs.Dialogs>();
            //Kit.Tools.Container.Register<IScreenManager, ScreenManagerService>();
            //Kit.Tools.Container.Register<Kit.Controls.CrossImage.CrossImageExtensions, Kit.WPF.Controls.CrossImage.CrossImageExtensions>();
            //Kit.Tools.Container.Register<IBarCodeBuilder, BarCodeBuilder>();
            //Kit.Tools.Container.Register<IClipboardService, ClipboardService>();
#if NET6_0_OR_GREATER
            //Kit.Tools.Container.Register<Plugin.DeviceInfo.Abstractions.IDeviceInfo,>();
#else
            //Kit.Tools.Container.Register<Plugin.DeviceInfo.Abstractions.IDeviceInfo, Plugin.DeviceInfo.DeviceInfoImplementation>();
#endif

            Log.Init(loggerFactory: (log) => (new LoggerConfiguration()
                    // Set default log level limit to Debug
                    .MinimumLevel.Verbose()
                    // Enrich each log entry with memory usage and thread ID
                    // .Enrich.WithMemoryUsage()
                    //.Enrich.WithThreadId()
                    // Write entries to Android log (Nuget package Serilog.Sinks.Xamarin)
                    .WriteTo.Console().MinimumLevel.Verbose()
                    // Create a custom logger in order to set another limit,
                    // particularly, any logs from Information level will also be written into a rolling file
                    .WriteTo.Logger(config =>
                    config
                            .MinimumLevel.Verbose()
                            .WriteTo.File(log.LoggerPath, retainedFileCountLimit: 7,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(500))
                    )
                    // And create another logger so that logs at Fatal level will immediately send email
                    .WriteTo.Logger(config =>
                        config
                            .MinimumLevel.Fatal()
                            .WriteTo.File(log.CriticalLoggerPath, retainedFileCountLimit: 1,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(500))
                    )).CreateLogger(), CriticalAction: CriticalAlert);
            base.Init();
            return this;
        }
    }
}