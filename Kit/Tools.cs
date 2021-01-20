using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        static AbstractTools currentInstance;
        public static AbstractTools Instance
        {
            get
            {
                if (currentInstance == null)
                    throw new ArgumentException("Please Init Plugin.Xamarin.Tools before using it");

                return currentInstance;
            }
            set => currentInstance = value;
        }

    }
}
