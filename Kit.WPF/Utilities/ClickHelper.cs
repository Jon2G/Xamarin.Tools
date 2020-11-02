using SQLHelper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Xamarin.Tools.WPF.Utilities
{
    public static class ClickHelper
    {
        private static ulong Ultimo { get; set; }

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();

        static ClickHelper()
        {
            ClickHelper.Ultimo = 0;
        }
        public static bool EsValido()
        {

            ulong Actual = GetTickCount64();// Environment.TickCount;
            //Log.LogMe($"Actual:{Actual},Utlimo:{Ultimo}");

            ulong Diferencia = Actual - Ultimo;

            if (Diferencia > 300)
            {
                ClickHelper.Ultimo = Actual;
                //Log.LogMe($"OK Diferencia:{Diferencia}");
                return true;
            }
            //Log.LogMe($"KO Diferencia:{Diferencia}");
            return false;
        }
    }
}
