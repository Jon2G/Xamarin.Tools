using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kit.Sql.Helpers
{
    public static class SQLHelper
    {
        public static void Load_e_sqlite_3()
        {
            bool is64 = Environment.Is64BitOperatingSystem;
            FileInfo dll = new FileInfo($"{Tools.Instance.LibraryPath}\\{(is64 ? "x64" : "x86")}\\sqlite3.dll");
            if (dll.Exists)
                Assembly.Load(dll.FullName);
        }
        public static string FormatTime(TimeSpan TimeSpan)
        {
            return $"{TimeSpan:hh}:{TimeSpan:mm}:{TimeSpan:ss}";
        }
        public static string FormatTime(DateTime TimeSpan)
        {
            //using (IDbConnection lite = Conecction())
            //{
            //'2020-09-17T12:27:55'  Formato universal de fecha y hora sql server
            return TimeSpan.ToString("yyyy-MM-ddTHH:mm:ss");
            //}
        }
        public static bool IsInjection(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains('\''))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsInjection(params string[] values)
        {
            foreach (string value in values)
            {
                if (IsInjection(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static object NullIfEmpty(string value)
        {
            if (string.IsNullOrEmpty(value?.Trim()))
            {
                return DBNull.Value;
            }
            return value;
        }

        public static bool ToBool(object valor)
        {
            return ToBool(valor, false);
        }
        public static bool ToBool(object valor, bool _default = false)
        {
            if (IsNull(valor))
            {
                return _default;
            }
            switch (valor)
            {
                case short:
                case Int32:
                    return Convert.ToInt32(valor) == 1;
                case bool boleano:
                    return boleano;
                default:
                    return _default;
            }
        }
        public static bool? ToBool(object valor, bool? _default = null)
        {
            if (IsNull(valor))
            {
                return _default;
            }
            switch (valor)
            {
                case short:
                case Int32:
                    return Convert.ToInt32(valor) == 1;
                case bool boleano:
                    return boleano;
                default:
                    return _default;
            }
        }

        public static bool IsNull(object value)
        {
            return value == DBNull.Value || value == null;
        }
        public static object IfNull(object value, object ifnull)
        {
            return IsNull(value) ? ifnull : value;
        }

    }
}
