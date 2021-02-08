using Kit.Services.Interfaces;
using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Kit.NetCore
{
    public class ToolsImplementation : AbstractTools
    {
        public override ITools Init(IDeviceInfo DeviceInfo, string LogDirectory = "Logs", bool AlertAfterCritical = false)
        {
            this.CustomMessageBox = new Services.ICustomMessageBox.CustomMessageBoxService();
            base.Init(DeviceInfo, LogDirectory, AlertAfterCritical);
            Debugging = Debugger.IsAttached;
            if (AlertAfterCritical)
            {
                Log.Init(LogDirectory, CriticalAlert);
            }
            else
            {
                Log.Init(LogDirectory);
            }
            return this;
        }

        public override AbstractTools SetDebugging(bool Debugging)
        {
            this.Debugging = Debugging;
            Sqlh.Instance?.SetDebugging(Debugging);
            return this;
        }
        public override void CriticalAlert(object sender, EventArgs e)
        {
            //DependencyService.Get<ICustomMessageBox>()
            //    .ShowOK(sender.ToString(), "Alerta", "Entiendo", Shared.Enums.CustomMessageBoxImage.Error);
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
                    Log.LogMe(ex, "Ventana padre");
                }

                return a.IsActive ? a : null;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al determinar la ventana padre");
                return null;
            }
        }
    }
}
