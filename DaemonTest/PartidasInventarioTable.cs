using Kit.Daemon.VersionControl;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaemonTest
{
    public class PartidasInventarioTable : IVersionControlTable
    {
        public PartidasInventarioTable(BaseSQLHelper SQLH) : base(SQLH, 7)
        {

        }
        public override string TableName => "PARTIDAS_INVENTARIO_MOVIL";

        protected override void CreateTable(SqlServer SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE PARTIDAS_INVENTARIO_MOVIL
                    (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ID_INVENTARIO INT DEFAULT NULL ,--FOREIGN KEY REFERENCES INVENTARIO_MOVIL,
                    ARTICULO NVARCHAR(30) FOREIGN KEY REFERENCES PRODS  DEFAULT NULL,
                    CANTIDAD FLOAT DEFAULT 0  
                    )", System.Data.CommandType.Text, false);
        }

        protected override void CreateTable(SqLite SQLH)
        {
            return;
        }
    }
}
