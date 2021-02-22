using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Services.Interfaces;
using Serilog;


namespace Kit
{
    public class AbstractTools : ITools
    {
        public Kit.Services.Interfaces.ICustomMessageBox CustomMessageBox;
        private string _LibraryPath;

        public string LibraryPath => _LibraryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        protected AbstractTools()
        {

        }

        public static AbstractTools Instance => Tools.Instance;

        public virtual ITools Init(IDeviceInfo DeviceInfo)
        {
            Device.Init(DeviceInfo);

           

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

        private static readonly Lazy<bool> _IsInDesingMode =
            new Lazy<bool>(IsDesigning, System.Threading.LazyThreadSafetyMode.PublicationOnly);
        public static bool IsInDesingMode
        {
            get
            {
                bool ret = _IsInDesingMode.Value;
                return ret;
            }
        }

        private static bool IsDesigning()
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
