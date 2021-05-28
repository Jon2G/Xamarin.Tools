using Kit.Services.Interfaces;
using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Kit.Enums;
using Kit.NetCore.Controls.CrossImage;
using Kit.NetCore.Services;
using Kit.NetCore.Services.ICustomMessageBox;
using Plugin.DeviceInfo.Abstractions;
using Serilog;


namespace Kit.NetCore
{
    public class ToolsImplementation : AbstractTools
    {
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.NetCore;
        
        public static Kit.NetCore.ToolsImplementation Instance => Tools.Instance as Kit.NetCore.ToolsImplementation;
        public override string LibraryPath => Environment.CurrentDirectory;
        public override string TemporalPath => Path.GetTempPath();

        public override void Init()
        {
            Init(new CustomMessageBoxService(),
                new SynchronizeInvoke(), null,
                new ImageExtensions(), new BarCodeBuilder());

            Log.Init().SetLogger((new LoggerConfiguration()
                // Set default log level limit to Debug
                .MinimumLevel.Debug()
                // Enrich each log entry with memory usage and thread ID
                // .Enrich.WithMemoryUsage()
                //.Enrich.WithThreadId()
                // Create a custom logger in order to set another limit,
                // particularly, any logs from Information level will also be written into a rolling file
                .WriteTo.Async(x => x.Logger(config =>
                      config
                          .MinimumLevel.Information()
                          .WriteTo.Async(x => x.File(Log.Current.LoggerPath, retainedFileCountLimit: 7))
                ))
                // And create another logger so that logs at Fatal level will immediately send email
                .WriteTo.Logger(config =>
                    config
                        .MinimumLevel.Fatal()
                        .WriteTo.Async(x => x.File(Log.Current.CriticalLoggerPath, retainedFileCountLimit: 1))
                )).CreateLogger(), CriticalAlert);
        }

        public override void CriticalAlert(object sender, EventArgs e)
        {
            Log.Logger.Fatal(sender?.ToString());
        }

        public Window VentanaPadre()
        {
            if (IsInDesingMode)
            {
                return null;
            }
            try
            {
                Window a = (from nic in Application.Current.Windows.OfType<Window>()
                            where nic.IsActive //&& nic.GetType() != typeof(Chat)
                            select nic).FirstOrDefault();
                if (!(a is null)) return a.IsActive ? a : null;
                a = Application.Current.Windows.OfType<Window>().FirstOrDefault();
                if (a != null && a.IsActive) return a.IsActive ? a : null;
                try
                {
                    a?.Show();
                }
                catch (Exception ex)
                {
                    // ignored
                    Log.Logger.Error(ex, "Ventana padre");
                }

                return a.IsActive ? a : null;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al determinar la ventana padre");
                return null;
            }
        }
    }
}
