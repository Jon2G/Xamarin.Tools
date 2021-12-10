using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Kit.Entity
{
    public static class SqLiteExtensions
    {
        public static SqliteConnection SqliteConnectionFromFile(this FileInfo file)
        {
            SQLitePCL.Batteries_V2.Init();
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = file.FullName
            };
            return new SqliteConnection(builder.ConnectionString);
        }
        public static bool ViewExists(this SqliteConnection connection, string viewName)
        {
            return connection.Exists($"SELECT name FROM sqlite_master WHERE type='view' AND name='{viewName}'");
        }

        public static bool TableExists(this SqliteConnection connection, string TableName)
        {
            return connection.Exists($"SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}';");
        }

        public static FileInfo DatabaseFile(this SqliteConnection connection)
        {
            return new FileInfo(connection.ConnectionString);
        }
    }
}
