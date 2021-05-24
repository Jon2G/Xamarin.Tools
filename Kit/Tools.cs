using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kit
{
    public partial class Tools
    {
        public static bool IsInited => currentInstance != null;

        public static void Set(AbstractTools Instance)
        {
            currentInstance = Instance;
        }

        private static AbstractTools currentInstance;

        public static AbstractTools Instance
        {
            get
            {
                if (currentInstance == null)
                {
                    return null;
                }
                //throw new ArgumentException("Please Init Plugin.Xamarin.Tools before using it");

                return currentInstance;
            }
            set => currentInstance = value;
        }

        public static bool Debugging => Debugger.IsAttached;

        protected static void BaseInit()
        {
            if (Tools.Instance.RuntimePlatform != Enums.RuntimePlatform.WPF)
            {
                DirectoryInfo TempDirectory = new DirectoryInfo(Instance.TemporalPath);
                if (!TempDirectory.Exists)
                {
                    TempDirectory.Create();
                }
            }
        }
    }
}