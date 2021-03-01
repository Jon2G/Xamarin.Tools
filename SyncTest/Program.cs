using System;
using System.Data.SQLite;
using System.IO;
using Kit;
using Kit.Daemon;
using Kit.Sql.Sync;
using SQLServer;
using SyncTest.Models;

namespace SyncTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Kit.WPF.Tools.Init();
            using (SQLServerConnection con =
                new SQLServerConnection("RABBIT_RESPALDO_REAL", "192.168.0.2\\SQLEXPRESS", "1433", "sa", "12345678"))
            {
                using (Kit.Sql.Sqlite.SQLiteConnection lite = new Kit.Sql.Sqlite.SQLiteConnection(Path.Combine(Tools.Instance.LibraryPath, "TestDb.db"), 110))
                {
                    lite.CheckTables(typeof(Prods));
                    Daemon.Current.Configure(lite, con, lite.DBVersion)
                        .SetSchema(typeof(Prods));
                    Daemon.Current.Awake();
                }
            }

            Console.ReadKey();
        }


    }
}
