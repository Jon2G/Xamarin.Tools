using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Kit;
using Kit.Daemon;
using Kit.Sql.SqlServer;
using SyncTest.Models;

namespace SyncTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Kit.WPF.Tools.Init();
            //using (SQLServerConnection con =
            //    new SQLServerConnection("RABBIT_RESPALDO_REAL", "192.168.0.2\\SQLEXPRESS", "1433", "sa", "12345678"))
            //{
            //    using (Kit.Sql.Partitioned.SQLiteConnection lite =
            //        new Kit.Sql.Partitioned.SQLiteConnection(new DirectoryInfo(Tools.Instance.LibraryPath), 115))
            //    {
            //        lite.CheckTables(typeof(Prods));
            //        Daemon.Current.Configure(lite, con, lite.DBVersion)
            //            .SetSchema(typeof(Prods));
            //        Daemon.Current.Awake();

            //        UpdateTest update = new UpdateTest();
            //        update.Update(lite);
            //    }
            //}
            using (Kit.Sql.Sqlite.SQLiteConnection lite =
                new Kit.Sql.Sqlite.SQLiteConnection(new FileInfo(Path.Combine(Tools.Instance.LibraryPath, "root.db")), 116))
            {
                lite.CheckTables(typeof(Prods));
                for (int i = 1; i <= 100; i++)
                {
                    Prods P = new Prods()
                    {
                        Articulo = "ABC"
                    };
                    lite.Insert(P);
                    Log.Logger.Debug("PK=>{{{0}}}", P.Id);
                    Debug.Assert(P.Id == i, "Wrong pk", "WARNING PK=>{0}!={1}", P.Id, i);
                    P.Articulo = "UPDATED";
                    lite.InsertOrReplace(P);
                    Log.Logger.Debug("REPLACE PK=>{{{0}}}", P.Id);
                    Debug.Assert(P.Id == i, "Wrong pk", "WARNING PK=>{0}!={1}", P.Id, i);
                }
            }
            Console.ReadKey();
        }


    }
}
