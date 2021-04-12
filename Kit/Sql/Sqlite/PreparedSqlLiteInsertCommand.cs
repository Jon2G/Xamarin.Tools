using System;
using Kit.Sql.Exceptions;
using SQLitePCL;

namespace Kit.Sql.Sqlite
{
    /// <summary>
    /// Since the insert never changed, we only need to prepare once.
    /// </summary>
    class PreparedSqlLiteInsertCommand : IDisposable
    {
        bool Initialized;

        SQLiteConnection Connection;
        public sqlite3 Handle;
        string CommandText;

       public sqlite3_stmt Statement;
        static readonly sqlite3_stmt NullStatement = default (sqlite3_stmt);

        public PreparedSqlLiteInsertCommand (SQLiteConnection conn, string commandText)
        {
            Connection = conn;
            CommandText = commandText;
        }

        public int ExecuteNonQuery (object[] source)
        {
            if (Initialized && Statement == NullStatement) {
                throw new ObjectDisposedException (nameof (PreparedSqlLiteInsertCommand));
            }
            if(Connection.IsClosed)
                Connection.RenewConnection();

            if (Connection.Trace) {
                Connection.Tracer?.Invoke ("Executing: " + CommandText);
            }

            var r = SQLite3.Result.OK;

            if (!Initialized) {
                Statement = SQLite3.Prepare2 (Handle, CommandText);
                Initialized = true;
            }

            //bind the values.
            if (source != null) {
                for (int i = 0; i < source.Length; i++) {
                    SQLiteCommand.BindParameter (Statement, i + 1, source[i], Connection.StoreDateTimeAsTicks, Connection.DateTimeStringFormat, Connection.StoreTimeSpanAsTicks);
                }
            }
            r = SQLite3.Step (Statement);

            if (r == SQLite3.Result.Done)
            {
                int rowsAffected = SQLite3.Changes(Handle);
                SQLite3.Reset(Statement);
                return rowsAffected;
            }
            else if (r == SQLite3.Result.Error)
            {
                string msg = SQLite3.GetErrmsg(Handle);
                SQLite3.Reset(Statement);
                throw SQLiteException.New(r, msg);
            }
            else if (r == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode(Handle) == SQLite3.ExtendedResult.ConstraintNotNull)
            {
                SQLite3.Reset(Statement);
                throw NotNullConstraintViolationException.New(r, SQLite3.GetErrmsg(Handle));
            }
            else if (r == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode(Handle) == SQLite3.ExtendedResult.ConstraintUnique)
            {
                SQLite3.Reset(Statement);
                throw NotNullConstraintViolationException.New(r, SQLite3.GetErrmsg(Handle));
            }
            else
            {
                SQLite3.Reset(Statement);
                throw SQLiteException.New(r, SQLite3.GetErrmsg(Handle));
            }
        }

        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool disposing)
        {
            var s = Statement;
            Statement = NullStatement;
            Connection = null;
            if (s != NullStatement) {
                SQLite3.Finalize (s);
            }
        }

        ~PreparedSqlLiteInsertCommand ()
        {
            Dispose (false);
        }
    }
}