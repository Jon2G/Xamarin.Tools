using Kit.Daemon.Abstractions;
using Kit.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Sqlite;
using SQLServer;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers =true)]
    public class DaemonVersionTable : IVersionControlTable
    {
        public const string Name = "DAEMON_VERSION";
        public DaemonVersionTable(SQLServerConnection SQLH) : base(SQLH, 1)
        {

        }
        public override string TableName => Name;

        protected override void CreateTable(SQLServerConnection SQLH)
        {
            if (SQLH.TableExists("DESCARGAS_VERSIONES"))
                SQLH.EXEC("DROP TABLE DESCARGAS_VERSIONES");

            if (SQLH.TableExists("DISPOSITVOS_TABLETS"))
                SQLH.EXEC("DROP TABLE DISPOSITVOS_TABLETS");

            if (SQLH.TableExists("VERSION_CONTROL"))
                SQLH.EXEC("DROP TABLE VERSION_CONTROL");

            if (SQLH.TableExists("TRIGGERS_INFO"))
                SQLH.EXEC("DROP TABLE TRIGGERS_INFO");

            SQLH.EXEC(@"CREATE TABLE DAEMON_VERSION (
                  ID INT IDENTITY(1,1) PRIMARY KEY,
                  TABLA VARCHAR(100) DEFAULT '',
                  VERSION VARCHAR(100) DEFAULT '0.0.0');");

        }
        public static void SaveVersion(SqlBase SQLH, string tableName)
        {
            SQLH.EXEC($"INSERT INTO DAEMON_VERSION (TABLA,VERSION) VALUES('{tableName}','{Daemon.Current.DaemonConfig.DbVersion}');");
        }
        public static int GetVersion(SqlBase SQLH, string tableName)
        {
            if (!SQLH.TableExists(Name))
            {
                return 0;
            }
            return SQLH.Single<int>($"SELECT VERSION FROM DAEMON_VERSION WHERE TABLA='{tableName}'");
        }

        protected override void CreateTable(SQLiteConnection SQLH)
        {
            //Just for sqlserver
            return;
        }


    }
}
