using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncService.Daemon.VersionControl
{
    public class VersionControlTable : IVersionControlTable
    {
        private const string TableName = "VERSION_CONTROL";
        string IVersionControlTable.TableName => TableName;

        public void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE VERSION_CONTROL
                    (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ACCION char(1),
                    TABLA  varchar(50),
                    CAMPO VARCHAR(50),
                    LLAVE sql_variant);");
        }

        public void CreateTable(SQLHLite SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE VERSION_CONTROL
                    (
                    ID INTEGER PRIMARY KEY,
                    ACCION TEXT,
                    TABLA  TEXT,
                    CAMPO TEXT,
                    LLAVE BLOB);");
        }
    }
}
