using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kit.Services.Interfaces;
using Kit.Sql;
using Kit.Sql.Helpers;

namespace Kit
{
    public class AbstractTools : ITools
    {
        public Kit.Services.Interfaces.ICustomMessageBox CustomMessageBox;
        public bool Debugging { get; protected set; }
        public string DeviceId { get; protected set; }
        private string _LibraryPath;
        public string LibraryPath
        {
            get => _LibraryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
        protected AbstractTools()
        {

        }
        public static AbstractTools Instance => Tools.Instance;

        public virtual ITools Init(IDeviceInfo DeviceInfo, string LogDirectory = "Logs", bool AlertAfterCritical = false)
        {
            this.DeviceId = DeviceInfo.DeviceId;
            if (AlertAfterCritical)
            {
                Log.Init(LogDirectory,CriticalAlert);
            }
            else
            {
                Log.Init(LogDirectory);
            }
            return this;
        }

        public virtual AbstractTools SetDebugging(bool Debugging)
        {
            this.Debugging = Debugging;
            Sqlh.Instance?.SetDebugging(Debugging);
            return this;
        }
        public virtual void CriticalAlert(object sender, EventArgs e)
        {
            CustomMessageBox.ShowOK(sender.ToString(), "Alerta", "Entiendo");
        }
        public AbstractTools SetLibraryPath(string LibraryPath)
        {
            _LibraryPath = LibraryPath;
            return this;
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

            //if (Device.RuntimePlatform == Device.UWP)
            //{
            //    return false;
            //}

            string name = Process.GetCurrentProcess().ProcessName;
            name = name?.Trim()?.ToUpper();
            if (name == "XDESPROC" || name == "DEVENV")
            {
                return true;
            }
            // MessageBox.Show(name);
            return false;
        }
    }
}
