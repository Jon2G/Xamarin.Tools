using Kit.Services.Interfaces;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Kit.Services;
using Application = System.Windows.Application;
using Kit.Sql.Helpers;
using Serilog;

namespace Kit.WPF
{
    public class ToolsImplementation : AbstractTools
    {
        public override AbstractTools Init(IDeviceInfo DeviceInfo, ICustomMessageBox CustomMessageBox)
        {
            base.Init(DeviceInfo, new Services.ICustomMessageBox.CustomMessageBoxService());
 
            Log.Init().SetLogger((new LoggerConfiguration()
                // Set default log level limit to Debug
                .MinimumLevel.Debug()
                // Enrich each log entry with memory usage and thread ID
                // .Enrich.WithMemoryUsage()
                //.Enrich.WithThreadId()
                // Write entries to Android log (Nuget package Serilog.Sinks.Xamarin)
                .WriteTo.Console()
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
            MessageBox.Show(sender.ToString(), "Alerta", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #region UWP Especific
        public static ToolsImplementation UWPInstance
        {
            get => Kit.Tools.Instance as ToolsImplementation;
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


        #endregion
    }
}
