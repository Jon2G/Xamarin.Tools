using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kit
{
    public abstract class AbstractTools : ITools
    {
        public bool Debugging { get; protected set; }
        private string _LibraryPath;
        public string LibraryPath
        {
            get => _LibraryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
        public AbstractTools()
        {

        }
        public static AbstractTools Instance => Tools.Instance;
        public abstract ITools InitAll(string LogDirectory, bool AlertAfterCritical = false);
        public abstract ITools InitLoggin(string LogDirectory = "Logs", bool AlertAfterCritical = false);
        public abstract AbstractTools SetDebugging(bool Debugging);
        public virtual void CriticalAlert(object sender, EventArgs e)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(sender.ToString(), "Alerta", "Entiendo");
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
