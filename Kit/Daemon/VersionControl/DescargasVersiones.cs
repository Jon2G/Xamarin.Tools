using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon.VersionControl
{
    public class DescargasVersiones : IVersionControlTable
    {
        private const string TableName = "DESCARGAS_VERSIONES";
        string IVersionControlTable.TableName => TableName;

        public void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE DESCARGAS_VERSIONES
                (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                ID_DESCARGA INT, --FOREIGN KEY REFERENCES VERSION_CONTROL,
                ID_DISPOSITIVO VARCHAR(100) FOREIGN KEY REFERENCES DISPOSITVOS_TABLETS)");
        }

        public void CreateTable(SQLHLite SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE DESCARGAS_VERSIONES
                (
                ID INTEGER PRIMARY KEY,
                ID_DESCARGA INTEGER,
                ID_DISPOSITIVO TEXT)");
        }
    }
}
