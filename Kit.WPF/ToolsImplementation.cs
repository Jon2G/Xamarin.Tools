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

namespace Kit.WPF
{
    public class ToolsImplementation : AbstractTools
    {
        public override ITools Init(string LogDirectory = "Logs", bool AlertAfterCritical = false)
        {
            this.CustomMessageBox = new Services.ICustomMessageBox.CustomMessageBoxService();
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
            MessageBox.Show(sender.ToString(), "Alerta", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #region UWP Especific
        public static ToolsImplementation UWPInstance
        {
            get =>Kit.Tools.Instance as ToolsImplementation;
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


        #endregion
    }
}
