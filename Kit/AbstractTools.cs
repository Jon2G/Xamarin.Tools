using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using Serilog;


namespace Kit
{
    public abstract class AbstractTools
    {
        public Kit.Services.Interfaces.ICustomMessageBox CustomMessageBox { get; private set; }
        public Kit.Services.Interfaces.ISynchronizeInvoke SynchronizeInvoke { get; private set; }
        public Kit.Services.Interfaces.IScreenManager ScreenManager { get; private set; }
        public IBarCodeBuilder BarCodeBuilder { get; private set; }
        public Kit.Controls.CrossImage.CrossImageExtensions ImageExtensions { get; private set; }

        private string _LibraryPath;

        public string LibraryPath => _LibraryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string TemporalPath => Path.Combine(LibraryPath, "..", "tmp");

        protected AbstractTools()
        {

        }

        public static AbstractTools Instance => Tools.Instance;


        public abstract void Init();
        protected AbstractTools Init(IDeviceInfo DeviceInfo,
                ICustomMessageBox CustomMessageBox, ISynchronizeInvoke SynchronizeInvoke,IScreenManager ScreenManager,
                Kit.Controls.CrossImage.CrossImageExtensions ImageExtensions,IBarCodeBuilder BarCodeBuilder) 
        {
            this.SynchronizeInvoke = SynchronizeInvoke;
            this.CustomMessageBox = CustomMessageBox;
            this.ScreenManager = ScreenManager;
            this.ImageExtensions = ImageExtensions;
            this.BarCodeBuilder = BarCodeBuilder;
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

        public abstract RuntimePlatform RuntimePlatform { get; }

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
