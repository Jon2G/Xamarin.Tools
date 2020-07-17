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
    }
}
