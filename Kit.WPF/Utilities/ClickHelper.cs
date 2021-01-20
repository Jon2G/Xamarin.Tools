﻿using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kit.WPF.Utilities
{
    public static class ClickHelper
    {
        private static ulong Ultimo { get; set; }

        [DllImport("kernel32")]
        extern static ulong GetTickCount64();

        static ClickHelper()
        {
            Ultimo = 0;
        }
        public static bool EsValido()
        {

            ulong Actual = GetTickCount64();// Environment.TickCount;
            //Log.LogMe($"Actual:{Actual},Utlimo:{Ultimo}");

            ulong Diferencia = Actual - Ultimo;

            if (Diferencia > 300)
            {
                Ultimo = Actual;
                //Log.LogMe($"OK Diferencia:{Diferencia}");
                return true;
            }
            //Log.LogMe($"KO Diferencia:{Diferencia}");
            return false;
        }
    }
}
