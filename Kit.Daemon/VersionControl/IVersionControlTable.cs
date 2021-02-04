using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Linker;
using Kit.Sql.Interfaces;
using Kit.Sql.Helpers;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public abstract class IVersionControlTable : IComparable<IVersionControlTable>
    {
        public readonly int Priority;
        protected readonly BaseSQLHelper SQLH;
        public abstract string TableName { get; }
        public void CreateTable()
        {
            switch (SQLH)
            {
                case SqlServer sqlserver:
                    CreateTable(sqlserver);
                    SaveVersion();
                    break;
                case SqLite sqlite:
                    CreateTable(sqlite);
                    break;
            }
        }
        protected abstract void CreateTable(SqlServer SQLH);
        protected abstract void CreateTable(SqLite SQLH);

        public IVersionControlTable(BaseSQLHelper SQLH, int Priority)
        {
            this.SQLH = SQLH;
            this.Priority = Priority;
        }
        public virtual void SaveVersion()
        {
            DaemonVersionTable.SaveVersion(SQLH, TableName);
        }
        public virtual ulong GetVersion()
        {
            return DaemonVersionTable.GetVersion(SQLH, TableName);
        }
        public virtual void DropTable()
        {
            if (SQLH.TableExists(TableName))
            {
                SQLH.EXEC($"DROP TABLE {TableName}");
            }

        }

        public int CompareTo(IVersionControlTable other)
        {
            return this.Priority - other.Priority;
        }
    }
}
