using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Kit;
using SyncTest.Models;

namespace SyncTest
{
    internal class PrimaryKeyTest
    {
        private readonly Kit.Sql.Sqlite.SQLiteConnection lite;
        public PrimaryKeyTest()
        {
          lite =
                new Kit.Sql.Sqlite.SQLiteConnection(new FileInfo(Path.Combine(Tools.Instance.LibraryPath, "root.db")),
                    116);
          lite.CheckTables(typeof(Prods));
        }
        public async void Run(int i = 1)
        {
            for (int j = 0; j < 10; j++, i++)
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
            await Task.Delay(500);
            lite.Close();
            Run(i);
        }
    }
}