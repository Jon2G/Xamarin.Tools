using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared
{
    public abstract class AbstractTools : ITools
    {
        public bool Debugging { get; protected set; }
        public string LibraryPath { get => System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); }
        public AbstractTools()
        {

        }

        public abstract ITools InitAll(string LogDirectory, bool AlertAfterCritical = false);
        public abstract ITools InitLoggin(string LogDirectory = "Logs", bool AlertAfterCritical = false);
        public abstract void SetDebugging(bool Debugging);
        public abstract void CriticalAlert(object sender, EventArgs e);
    }
}
