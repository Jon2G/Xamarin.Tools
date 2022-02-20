using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TableMapping = Kit.Sql.Base.TableMapping;
namespace Kit.Daemon.Abstractions
{
    public class SchemaTable
    {
        public string TableName { get; set; }
        private readonly List<TableMapping> Mappings;
        private readonly List<DaemonCompiledSetter> MappingsSetters;
        public readonly SyncDirection SyncDirection;
        public SchemaTable(string tableName, SyncDirection syncDirection) : this()
        {
            TableName = tableName;
            SyncDirection = syncDirection;
        }
        public SchemaTable()
        {
            MappingsSetters = new List<DaemonCompiledSetter>();
            Mappings = new List<TableMapping>();
        }
        public SchemaTable Add(TableMapping map)
        {
            Mappings.Add(map);
            MappingsSetters.Add(null);
            return this;
        }
        public Kit.Sql.SqlServer.TableMapping ForSqlServer()
        {
            return (Sql.SqlServer.TableMapping)this.Mappings.FirstOrDefault(x => x is Kit.Sql.SqlServer.TableMapping);
        }
        public Kit.Sql.Sqlite.TableMapping ForSqlite()
        {
            return (Sql.Sqlite.TableMapping)this.Mappings.FirstOrDefault(x => x is Kit.Sql.Sqlite.TableMapping);
        }
        public DaemonCompiledSetter CompiledSetterForSqlServer() 
        {
            int index = Mappings.IndexOf(ForSqlServer());
            return index >= 0 ? MappingsSetters[index] : null;
        }
        public DaemonCompiledSetter CompiledSetterForSqlite()
        {
            int index = Mappings.IndexOf(ForSqlite());
            return index >= 0 ? MappingsSetters[index] : null;

        }
        public DaemonCompiledSetter CompiledSetterFor(SqlBase sql)
        {
            switch (sql)
            {
                case SQLServerConnection:
                    return CompiledSetterForSqlServer();
                case SQLiteConnection:
                    return CompiledSetterForSqlite();
            }
            return null;
        }
        public TableMapping For(SqlBase sql)
        {
            switch (sql)
            {
                case SQLServerConnection:
                    return ForSqlServer();
                case SQLiteConnection:
                    return ForSqlite();
            }
            return null;
        }
        public void Add(TableMapping mapping,DaemonCompiledSetter daemonCompiledSetter)
        {
            int index =  Mappings.IndexOf(mapping);
            if (index >= 0)
            {
                MappingsSetters[index] = daemonCompiledSetter;
            }
        }
    }
}
