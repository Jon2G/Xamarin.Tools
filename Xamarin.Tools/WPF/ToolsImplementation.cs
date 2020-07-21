using Plugin.Xamarin.Tools.Shared;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Xamarin.Forms;
using Application = System.Windows.Application;

namespace Plugin.Xamarin.Tools.WPF
{
    public class ToolsImplementation : AbstractTools
    {
        public override ITools InitAll(string LogDirectory, bool AlertAfterCritical = false)
        {
            Debugging = Debugger.IsAttached;
            return InitLoggin(LogDirectory, AlertAfterCritical);
        }

        public override ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false)
        {
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

        public override void SetDebugging(bool Debugging)
        {
            SQLHelper.SQLHelper.Instance?.SetDebugging(Debugging);
        }
        public override void CriticalAlert(object sender, EventArgs e)
        {
            DependencyService.Get<Shared.Services.ICustomMessageBox>()
                .ShowOK(sender.ToString(), "Alerta", "Entiendo", Shared.Enums.CustomMessageBoxImage.Error);
        }
        #region UWP Especific
        public static ToolsImplementation UWPInstance
        {
            get => Shared.Tools.Instance as ToolsImplementation;
        }
        private bool? _IsInDesingMode;
        public bool IsInDesingMode
        {
            get
            {
                if (_IsInDesingMode is null)
                {
                    _IsInDesingMode = Designing();
                }
                return (bool)_IsInDesingMode;
            }
        }
        private bool Designing()
        {
            string name = Process.GetCurrentProcess().ProcessName;
            name = name?.Trim()?.ToUpper();
            if (name == "XDESPROC" || name == "DEVENV")
            {
                return true;
            }
            // MessageBox.Show(name);
            return false;
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
