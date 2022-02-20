using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.WPF
{
    public static class DotNetFrameworkLocator
    {
        public static string GetInstallationLocation()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey == null)
                    throw new Exception();

                var value = ndpKey.GetValue("InstallPath") as string;
                if (value != null)
                    return value;
                else
                    throw new Exception();
            }
        }
    }
}