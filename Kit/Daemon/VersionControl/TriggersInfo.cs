using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;
using Kit.Sql.Helpers;
using Kit.Sql.Sqlite;
using SQLServer;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public class TriggersInfo : IVersionControlTable
    {
        public TriggersInfo(SQLServerConnection SQLH) : base(SQLH, 2) { }
        public override string TableName => "TRIGGERS_INFO";
        protected override void CreateTable(SQLServerConnection SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE TRIGGERS_INFO
                (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                NAME VARCHAR(100) ,
                VERSION VARCHAR(100) 
                )");
        }

        protected override void CreateTable(SQLiteConnection SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE TRIGGERS_INFO
                (
                ID INT AUTOINCREMENT PRIMARY KEY,
                NAME TEXT ,
                VERSION TEXT
                )");
        }
    }
}
