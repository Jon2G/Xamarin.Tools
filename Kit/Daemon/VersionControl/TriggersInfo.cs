using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon.VersionControl
{
    public class TriggersInfo : IVersionControlTable
    {
        public TriggersInfo(SQLH SQLH) : base(SQLH, 2) { }
        public override string TableName => "TRIGGERS_INFO";
        protected override void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE TRIGGERS_INFO
                (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                NAME VARCHAR(100) ,
                VERSION VARCHAR(100) 
                )");
        }

        protected override void CreateTable(SQLHLite SQLH)
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
