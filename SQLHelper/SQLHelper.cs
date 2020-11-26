using System;
using System.Collections.Generic;
using System.Text;

namespace SQLHelper
{
    public class SQLHelper
    {
        public bool Debugging { get; protected set; }
        private string _LibraryPath;
        public string LibraryPath
        {
            get => _LibraryPath ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            set => _LibraryPath = value;
        }

        public SQLHelper(string LibraryPath, bool Debugging)
        {
            this.LibraryPath = LibraryPath;
            this.Debugging = Debugging;
        }

        public void SetDebugging(bool Debugging)
        {
            this.Debugging = Debugging;
        }


        public static SQLHelper Init(string LibraryPath, bool Debugging)
        {
            Set(new SQLHelper(LibraryPath, Debugging));
            return Instance;
        }

        private static void Set(SQLHelper Instance)
        {
            _Instance = Instance;
        }
        private static SQLHelper _Instance;
        public static SQLHelper Instance => _Instance;


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
