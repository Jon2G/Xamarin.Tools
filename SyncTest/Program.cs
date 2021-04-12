using System;
using System.Data.SQLite;
using Kit.Daemon;
using Kit.Sql.SqlServer;

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
            PrimaryKeyTest test = new PrimaryKeyTest();
            test.Run();
            
            Console.ReadKey();
        }


    }
}
