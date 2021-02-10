using Kit.Daemon.Abstractions;
using Kit.Enums;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kit.Daemon.Enums;
using Kit.Sql.Interfaces;
using Kit.Sql.Helpers;
using Kit.Sql.Linker;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public class VersionControlTable : IVersionControlTable
    {
        public VersionControlTable(BaseSQLHelper SQLH) : base(SQLH, 3) { }

        public override string TableName => "VERSION_CONTROL";

        protected override void CreateTable(SqlServer SQLH)
        {
            SQLH.EXEC(@"CREATE TABLE VERSION_CONTROL
                    (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ACCION char(1),
                    TABLA  varchar(50),
                    CAMPO VARCHAR(50),
                    LLAVE sql_variant);");
            if (Daemon.Current.Schema.HasDownloadTables)
            {
                ResetVersionControl(Daemon.Current.Schema.DownloadTables);
            }
        }

        protected override void CreateTable(SqLite SQLH)
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
                Trigger.CheckTrigger((SqlServer)SQLH, table, Daemon.Current.DaemonConfig.DbVersion);
            }
        }
    }
}
