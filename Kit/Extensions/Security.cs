using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Extensions
{
    public static class Security
    {
        public static string Decrypta(string cTexto, int nLlave = 777)
        {
            long nLargo;
            int n;
            char cCaracter;
            string NewCadena;
            int NewNumero;
            nLargo = cTexto.Trim().Length;
            NewCadena = "";
            NewNumero = (nLlave / 256) * 4;
            for (n = 0; n < nLargo; n++)
            {
                cCaracter = (char)(cTexto.Substring(n, 1)[0] - NewNumero);
                NewCadena += cCaracter;
            }
            return NewCadena;
        }

    }
}
