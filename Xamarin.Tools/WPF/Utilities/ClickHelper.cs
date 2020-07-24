using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.WPF.Utilities
{
    public static class ClickHelper
    {
        private static int Ultimo { get; set; }
        public static bool EsValido()
        {

            int Actual = Environment.TickCount;
            int Diferencia = Actual - Ultimo;

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
