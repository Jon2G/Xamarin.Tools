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
    public class InventarioMovilTable : IVersionControlTable
    {
        public InventarioMovilTable(BaseSQLHelper SQLH) : base(SQLH, 6)
        {

        }
        public override string TableName => "INVENTARIO_MOVIL";

        protected override void CreateTable(SqlServer SQLH)
        {
            SQLH.EXEC(
                @"CREATE TABLE INVENTARIO_MOVIL
                    (
                    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    GUID VARCHAR(MAX) NOT NULL DEFAULT NEWID(),
                    ID_DISPOSITIVO VARCHAR(100),
                    ARTICULOS_INVENTARIADOS INT NOT NULL DEFAULT 0,
                    LINEAS_INVENTARIADAS  INT NOT NULL DEFAULT 0,
                    FECHA DATETIME NOT NULL DEFAULT GETDATE(),
                    CONCENTRADO BIT NOT NULL DEFAULT 0,
                    ID_ALMACEN INT NOT NULL DEFAULT 0
                    );", System.Data.CommandType.Text, false);
        }

        protected override void CreateTable(SqLite SQLH)
        {
            return;
        }
    }
}
