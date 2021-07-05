using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Dialogs;
using Kit.Enums;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using Serilog;

namespace Kit
{
    public abstract class AbstractTools
    {

        public Kit.Services.Interfaces.ISynchronizeInvoke SynchronizeInvoke { get; private set; }
        public Kit.Services.Interfaces.IScreenManager ScreenManager { get; private set; }
        public IBarCodeBuilder BarCodeBuilder { get; private set; }
        public IDialogs Dialogs { get; private set; }
        public Kit.Controls.CrossImage.CrossImageExtensions ImageExtensions { get; private set; }

        public virtual string LibraryPath => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public virtual string TemporalPath => Path.Combine(LibraryPath, "..", "tmp");

        protected AbstractTools()
        {
        }

        public static AbstractTools Instance => Tools.Instance;

        public abstract void Init();

        protected AbstractTools Init(
                IDialogs Dialogs, ISynchronizeInvoke SynchronizeInvoke, IScreenManager ScreenManager,
                Kit.Controls.CrossImage.CrossImageExtensions ImageExtensions, IBarCodeBuilder BarCodeBuilder)
        {
            this.SynchronizeInvoke = SynchronizeInvoke;
            this.Dialogs = Dialogs;
            this.ScreenManager = ScreenManager;
            this.ImageExtensions = ImageExtensions;
            this.BarCodeBuilder = BarCodeBuilder;
            Device.Init();
            return this;
        }

        public virtual void CriticalAlert(object sender, EventArgs e)
        {
            this.Dialogs.CustomMessageBox.ShowOK(sender.ToString(), "Alerta", "Entiendo");
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