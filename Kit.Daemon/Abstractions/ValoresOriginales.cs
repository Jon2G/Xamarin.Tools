using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon.Abstractions
{
    public class ValoresOriginales
    {
        public readonly object[] Valores;
        public readonly int Index;
        public ValoresOriginales(int Index, int Lenght)
        {
            this.Index = Index;
            Valores = new object[Lenght];
        }

        public object this[int Index]
        {
            get => Valores[Index];
            set
            {
                Valores[Index] = value;
            }
        }
        public void Agregar(object Valor, int i)
        {
            if (Sqlh.IsNull(Valor))
            {
                Valor = null;
            }
            switch (Valor)
            {
                case TimeSpan time:
                    Valor = Sqlh.FormatTime(time);
                    break;
                case DateTime date:
                    Valor = Sqlh.FormatTime(date);
                    break;
            }
            //switch (Valor)
            //{
            //    case DBNull n:
            //        Valor = "NULL";
            //        break;
            //    case byte[] b:
            //        Valor = "'{Encoding.Unicode.GetString(b)}'";
            //        break;
            //    case string b:
            //        Valor = "'" + b.Trim() + "'";
            //        break;
            //    case double d:
            //        Valor = d;
            //        break;
            //    case float d:
            //        Valor = d;
            //        break;
            //    case decimal d:
            //        Valor = (double)d;
            //        break;
            //    case int d:
            //        Valor = d;
            //        break;
            //    case short d:
            //        Valor = d;
            //        break;
            //    case long d:
            //        Valor = d;
            //        break;
            //    case bool d:
            //        Valor = d ? 1 : 0;
            //        break;
            //    case byte d:
            //        Valor = (int)d;
            //        break;
            //    case TimeSpan d:
            //        Valor = "'" + d.ToString().Trim() + "'";
            //        break;
            //    case DateTime d:
            //        Valor = "'" + d.ToString("yyyy-MM-dd HH:MM:ss").Trim() + "'";
            //        break;
            //    default:
            //        Log.LogMeDemonio("Tipo de dato no esperado:" + Valor.GetType().Name);
            //        Valor = "'" + Valor + "'";
            //        break;
            //}
            Valores[i] = Valor;
        }
    }
}
