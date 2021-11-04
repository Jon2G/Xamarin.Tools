using System;
using System.Data.SqlClient;
using System.Linq;
using SQLitePCL;

namespace Kit.Sql.SqlServer
{
    /// <summary>
    /// Since the insert never changed, we only need to prepare once.
    /// </summary>
    internal class PreparedSqlServerInsertCommand : IDisposable
    {
        private bool Initialized = false;

        private SQLServerConnection Connection;

        private string CommandText;

        private sqlite3_stmt Statement;
        private static readonly sqlite3_stmt NullStatement = default(sqlite3_stmt);

        public PreparedSqlServerInsertCommand(SQLServerConnection conn, string commandText)
        {
            Connection = conn;
            CommandText = commandText;
        }

        public long ExecuteNonQueryAndRecoverLastScopeIdentity(SqlParameter[] source)
        {
            if (Initialized && Statement == NullStatement)
            {
                throw new ObjectDisposedException(nameof(PreparedSqlServerInsertCommand));
            }
            CommandText = this.CommandText + "; select SCOPE_IDENTITY();";
            Log.Logger.Debug("Executing:[{0}]", CommandText);
            //if (Connection.IsClosed)
            //{
            //    Connection.RenewConnection();
            //}
            Int64 result = 0;
            Connection.Con().Read(this.CommandText, (reader) =>
            {
                if (reader.Read())
                {
                    Log.Logger.Debug("Affected: {0}", reader.RecordsAffected);
                    result = Convert.ToInt64(reader[0]);
                }
            }, source);
            return 0;
        }

        private long LastInsertRowid(SqlConnection con)
        {
            using (SqlCommand cmd = new SqlCommand("select SCOPE_IDENTITY()", con))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Convert.ToInt64(reader[0]);
                    }
                }

                return 0;
            }
        }

        public int ExecuteNonQuery(SqlParameter[] source)
        {
            if (Initialized && Statement == NullStatement)
            {
                throw new ObjectDisposedException(nameof(PreparedSqlServerInsertCommand));
            }
            Log.Logger.Debug("Executing:[{0}]", this.CommandText);
            int RecordsAffected = Connection.Con().Execute(this.CommandText, source);
            Log.Logger.Debug("Affected: {0}", RecordsAffected);
            return RecordsAffected;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            var s = Statement;
            Statement = NullStatement;
            Connection = null;
            if (s != NullStatement)
            {
                SQLite3.Finalize(s);
            }
        }

        ~PreparedSqlServerInsertCommand()
        {
            Dispose(false);
        }
    }
}