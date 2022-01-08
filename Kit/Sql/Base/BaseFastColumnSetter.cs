using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Kit.Sql.Base
{
    public class BaseFastColumnSetter
    {
        public static Action<T, TConnection, int> GetFastSetter<T,TConnection>(SqlBase sql, Column column)
        {
            if (sql is SqlServer.SQLServerConnection)
            {
                return SqlServer.FastColumnSetter.GetFastSetter<T>(column) as Action<T, TConnection, int>;
            }
            else if (sql is Sqlite.SQLiteConnection con)
            {
                return Sqlite.FastColumnSetter.GetFastSetter<T>(conn: con, column: column) as Action<T, TConnection, int>;
            }
            return null;
        }
    }
}
