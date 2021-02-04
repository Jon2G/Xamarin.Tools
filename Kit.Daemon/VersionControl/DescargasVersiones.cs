using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Sql.Linker;
using Kit.Sql.Helpers;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public class DescargasVersiones : IVersionControlTable
    {
        public DescargasVersiones(SqlServer SQLH) : base(SQLH, 5) { }
        public override string TableName => "DESCARGAS_VERSIONES";


        protected override void CreateTable(SqlServer SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE DESCARGAS_VERSIONES
                (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                ID_DESCARGA INT, --FOREIGN KEY REFERENCES VERSION_CONTROL,
                ID_DISPOSITIVO VARCHAR(100) FOREIGN KEY REFERENCES DISPOSITVOS_TABLETS)");

            SQLH.EXEC($"DELETE FROM DESCARGAS_VERSIONES WHERE ID_DISPOSITIVO = '{Device.Current.DeviceId}'");
        }

        protected override void CreateTable(SqLite SQLH)
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
