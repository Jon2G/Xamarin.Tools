using System.Runtime.InteropServices;

namespace Kit.WPF.Extensions
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
