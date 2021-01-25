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
                case SQLH sqlserver:
                    CreateTable(sqlserver);
                    SaveVersion();
                    break;
                case SQLHLite sqlite:
                    CreateTable(sqlite);
                    break;
            }
        }
        protected abstract void CreateTable(SQLH SQLH);
        protected abstract void CreateTable(SQLHLite SQLH);

        public IVersionControlTable(BaseSQLHelper SQLH, int Priority)
        {
            this.SQLH = SQLH;
            this.Priority = Priority;
        }
        public virtual void SaveVersion()
        {
            DaemonVersionTable.SaveVersion(SQLH, TableName);
        }
        public virtual string GetVersion()
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
