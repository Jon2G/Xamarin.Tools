using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Linker;
using Kit.Sql.Helpers;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public class TriggersInfo : IVersionControlTable
    {
        public TriggersInfo(SqlServer SQLH) : base(SQLH, 2) { }
        public override string TableName => "TRIGGERS_INFO";
        protected override void CreateTable(SqlServer SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE TRIGGERS_INFO
                (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                NAME VARCHAR(100) ,
                VERSION VARCHAR(100) 
                )");
        }

        protected override void CreateTable(SqLite SQLH)
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
