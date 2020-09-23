﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
#if NETCOREAPP
#else 
using Xamarin.Forms;
#endif

namespace Plugin.Xamarin.Tools.Shared
{
    public abstract class AbstractTools : ITools
    {
        public bool Debugging { get; protected set; }
        private string _LibraryPath;
        public string LibraryPath
        {
            get => _LibraryPath ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
        public AbstractTools()
        {

        }

        public abstract ITools InitAll(string LogDirectory, bool AlertAfterCritical = false);
        public abstract ITools InitLoggin(string LogDirectory = "Logs", bool AlertAfterCritical = false);
        public abstract AbstractTools SetDebugging(bool Debugging);
        public abstract void CriticalAlert(object sender, EventArgs e);
        public AbstractTools SetLibraryPath(string LibraryPath)
        {
            this._LibraryPath = LibraryPath;
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
#if NETCOREAPP

#else 
            if (Device.RuntimePlatform == Device.UWP)
            {
                return false;
            }
#endif
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
