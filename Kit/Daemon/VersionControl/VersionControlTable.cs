using Kit.Daemon.Abstractions;
using Kit.Enums;
using SQLHelper;
using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kit.Daemon.VersionControl
{
    public class VersionControlTable : IVersionControlTable
    {
        public VersionControlTable(BaseSQLHelper SQLH) : base(SQLH, 3) { }

        public override string TableName => "VERSION_CONTROL";

        protected override void CreateTable(SQLH SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE VERSION_CONTROL
                    (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ACCION char(1),
                    TABLA  varchar(50),
                    CAMPO VARCHAR(50),
                    LLAVE sql_variant);");
            ResetVersionControl(Daemon.Current.Schema.Where(x => x.TableDirection != TableDirection.UPLOAD));
        }

        protected override void CreateTable(SQLHLite SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE VERSION_CONTROL
                    (
                    ID INTEGER PRIMARY KEY,
                    ACCION TEXT,
                    TABLA  TEXT,
                    CAMPO TEXT,
                    LLAVE BLOB);");
        }
        public void ResetVersionControl(IEnumerable<Table> Tables)
        {
            foreach (Table table in Tables)
            {
                if (!table.Fields.Any())
                {
                    return;
                }
                Trigger.CheckTrigger((SQLH)SQLH, table, Daemon.Current.DaemonConfig.DbVersion);
            }
        }
    }
}
