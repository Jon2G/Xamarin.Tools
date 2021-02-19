using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Interfaces;
using Kit.Sql.Helpers;
using Kit.Sql.Sqlite;
using SQLServer;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public abstract class IVersionControlTable : IComparable<IVersionControlTable>
    {
        public readonly int Priority;
        protected readonly SqlBase SQLH;
        public abstract string TableName { get; }
        public void CreateTable()
        {
            switch (SQLH)
            {
                case SQLServerConnection sqlserver:
                    CreateTable(sqlserver);
                    SaveVersion();
                    break;
                case SQLiteConnection sqlite:
                    CreateTable(sqlite);
                    break;
            }
        }
        protected abstract void CreateTable(SQLServerConnection SQLH);
        protected abstract void CreateTable(SQLiteConnection SQLH);

        public IVersionControlTable(SqlBase SQLH, int Priority)
        {
            this.SQLH = SQLH;
            this.Priority = Priority;
        }
        public virtual void SaveVersion()
        {
            DaemonVersionTable.SaveVersion(SQLH, TableName);
        }
        public virtual int GetVersion()
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
