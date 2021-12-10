using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kit.Db.Abstractions;
using Kit.Entity;
using Kit.Enums;
using Kit.Sql.Helpers;
using Kit.Sql.SqlServer;

namespace Kit.Entity
{
    public static class SqlServerExtensions
    {
        public static SqlServerInformation GetServerInformation(this SqlConnection connection)
        {
            Regex regex = new Regex(@"(?<Version>\d+).*");
            SqlServerInformation inf = new SqlServerInformation(Enums.SqlServerVersion.None, string.Empty, string.Empty);
            connection.Read("SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY('productlevel'), SERVERPROPERTY('edition')",
            (reader) =>
            {
                int ver = 0;
                string version = reader[0].ToString();
                var match = regex.Match(version);
                if (match.Success)
                {
                    version = match.Groups["Version"].Value;
                    ver = Convert.ToInt32(version);
                }
                inf = new SqlServerInformation((SqlServerVersion)ver, reader[1].ToString(), reader[2].ToString());
            });
            return inf;
        }

        public static void SetCacheIdentity(this SqlConnection con, bool enabled)
        {
            con.Execute($"ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE={(enabled ? "ON" : "OFF")};");
        }
        public static bool TableExists(this SqlConnection connection, string tableName)
        {
            return connection.Exists($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Tablename",
                new SqlParameter("Tablename", tableName));
        }
        public static bool ViewExists(this SqlConnection connection, string viewName)
        {
            return connection.Exists($"SELECT name FROM sys.views WHERE name = @ViewName",
                new SqlParameter("ViewName", viewName));
        }



    }
}