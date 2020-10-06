using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncService.Daemon.VersionControl
{
    public class DispositivosTablets : IVersionControlTable
    {
        private const string TableName = "DISPOSITVOS_TABLETS";
        string IVersionControlTable.TableName => TableName;

        public void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE DISPOSITVOS_TABLETS
                            (
                            ID INT IDENTITY(1,1) UNIQUE,
                            ID_DISPOSITIVO VARCHAR(100) PRIMARY KEY,
                            ULTIMA_CONEXION DATETIME,
                            AUTORIZADO BIT DEFAULT 0,
                            RECUERDA_USUARIO VARCHAR(100),
                            GEO_ID VARCHAR(500) NOT NULL DEFAULT NEWID()
                            )");
        }
    }
}
