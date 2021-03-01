using System;
using System.Data.SqlClient;
using System.Linq;
using Kit;
using SQLitePCL;

namespace SQLServer
{
    /// <summary>
    /// Since the insert never changed, we only need to prepare once.
    /// </summary>
    class PreparedSqlServerInsertCommand : IDisposable
    {
        bool Initialized=false;

        SQLServerConnection Connection;

        string CommandText;

        sqlite3_stmt Statement;
        static readonly sqlite3_stmt NullStatement = default(sqlite3_stmt);

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

            if (Connection.Trace)
            {
                Connection.Tracer?.Invoke("Executing: " + CommandText);
            }

            CommandText = this.CommandText + "; select SCOPE_IDENTITY();";
            Log.Logger.Debug("Executing:[{0}]", CommandText);

            if (Connection.IsClosed)
            {
                Connection.RenewConnection();
            }
            using (var con = Connection.Connection)
            {
                con.Open();
                using (var cmd = new SqlCommand(this.CommandText, con))
                {
                    if (source?.Any() ?? false)
                    {
                        cmd.Parameters.AddRange(source);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Log.Logger.Debug("Affected: {0}", reader.RecordsAffected);

                            return Convert.ToInt64(reader[0]);
                        }
                    }
                }
            }

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

            if (Connection.Trace)
            {
                Connection.Tracer?.Invoke("Executing: " + CommandText);
            }
            Log.Logger.Debug("Executing:[{0}]",this.CommandText);
            if (Connection.IsClosed)
            {
                Connection.RenewConnection();
            }

            using (var con = Connection.Connection)
            {
                con.Open();
                using (var cmd = new SqlCommand(this.CommandText, con))
                {
                    if (source?.Any() ?? false)
                    {
                        cmd.Parameters.AddRange(source);
                    }
                    int RecordsAffected= cmd.ExecuteNonQuery();
                    Log.Logger.Debug("Affected: {0}", RecordsAffected);
                    return RecordsAffected;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
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