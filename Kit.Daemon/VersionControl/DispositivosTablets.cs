using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;
using SQLHelper.Linker;
namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public class DispositivosTablets : IVersionControlTable
    {
        public DispositivosTablets(SQLH SQLH) :base(SQLH,4) { }
        public override string TableName => "DISPOSITVOS_TABLETS";

        protected override void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE DISPOSITVOS_TABLETS
                            (
                            ID INT IDENTITY(1,1) UNIQUE,
                            ID_DISPOSITIVO VARCHAR(100) PRIMARY KEY,
                            ULTIMA_CONEXION DATETIME,
                            AUTORIZADO BIT DEFAULT 0,
                            NOMBRE VARCHAR(MAX),
                            GEO_ID VARCHAR(500) NOT NULL DEFAULT NEWID()
                            )");
        }

        protected override void CreateTable(SQLHLite SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE DISPOSITVOS_TABLETS
                            (
                            ID INTEGER AUTOINCREMENT,
                            ID_DISPOSITIVO TEXT PRIMARY KEY,
                            ULTIMA_CONEXION TEXT,
                            AUTORIZADO INTEGER DEFAULT 0,
                            RECUERDA_USUARIO TEXT,
                            GEO_ID TEXT
                            )");
        }
    }
}
