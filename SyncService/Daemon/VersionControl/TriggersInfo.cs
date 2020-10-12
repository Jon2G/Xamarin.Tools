using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncService.Daemon.VersionControl
{
    public class TriggersInfo : IVersionControlTable
    {
        private const string TableName = "TRIGGERS_INFO";
        string IVersionControlTable.TableName => TableName;
        public void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE TRIGGERS_INFO
                (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                NAME VARCHAR(100) ,
                VERSION VARCHAR(100) 
                )");
        }

        public void CreateTable(SQLHLite SQLH)
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
