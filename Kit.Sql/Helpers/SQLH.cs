using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Kit.Sql.Helpers
{
    public class Sqlh
    {
        public bool Debugging { get; protected set; }
        private string _LibraryPath;
        public string LibraryPath
        {
            get => _LibraryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            set => _LibraryPath = value;
        }

        public Sqlh(string LibraryPath, bool Debugging)
        {
            this.LibraryPath = LibraryPath;
            this.Debugging = Debugging;
        }

        public void SetDebugging(bool Debugging)
        {
            this.Debugging = Debugging;
        }


        public static Sqlh Init(string LibraryPath, bool Debugging)
        {
            Set(new Sqlh(LibraryPath, Debugging));
            return Instance;
        }

        private static void Set(Sqlh Instance)
        {
            _Instance = Instance;
        }
        private static Sqlh _Instance;
        public static Sqlh Instance => _Instance;


        public static string FormatTime(TimeSpan TimeSpan)
        {
            return $"{TimeSpan:hh}:{TimeSpan:mm}:{TimeSpan:ss}";
        }
        public static string FormatTime(DateTime TimeSpan)
        {
            //using (SQLiteConnection lite = Conecction())
            //{
            //'2020-09-17T12:27:55'  Formato universal de fecha y hora sql server
            return TimeSpan.ToString("yyyy-MM-ddTHH:mm:ss");
            //}
        }

        public static object NullIfEmpty(string value)
        {
            if (string.IsNullOrEmpty(value?.Trim()))
            {
                return DBNull.Value;
            }
            return value;
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
                case int:
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
                case int:
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
