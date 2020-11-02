using SQLHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Kit.Services;
using Kit.Enums;
using Xamarin.Forms;


namespace Kit.UWP
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

        public override AbstractTools SetDebugging(bool Debugging)
        {
            this.Debugging = true;
            return this;
        }
        //public override void CriticalAlert(object sender, EventArgs e)
        //{
        //    CustomMessageBox.Current
        //        .ShowOK(sender.ToString(), "Alerta", "Entiendo", CustomMessageBoxImage.Error);
        //}
        #region UWP Especific
        public static ToolsImplementation UWPInstance
        {
            get => Kit.Tools.Instance as ToolsImplementation;
        }
        private bool? _IsInDesingMode;
        public new bool IsInDesingMode
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
        #endregion
    }
}
