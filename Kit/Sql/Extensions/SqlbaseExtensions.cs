using Kit.Sql.Base;
using Kit.Sql.Readers;
using Kit.Sql.SqlServer;
using Kit.Sql.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Kit
{
    public static class SqlbaseExtensions
    {
        public static void Read(this SqlBase connection, string sql, Action<IReader> OnRead)
        {
            switch (connection)
            {
                case SQLiteConnection lite:
                    SqliteExtensions.Read(lite, sql, OnRead);
                    return;
                case SQLServerConnection server:
                    SqlServerExtensions.Read(server, sql, OnRead);
                    return;
            }
        }
    }
}
