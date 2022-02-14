using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kit.Daemon.Enums;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.SqlServer;
using static Kit.Sql.Base.BaseOrm;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Daemon.Abstractions
{
    [Preserve(AllMembers = true)]
    public class Schema
    {
        public readonly Dictionary<string, SchemaTable> Tables;
        private HashSet<string> DeniedTables;
        public readonly bool HasDownloadTables;
        public readonly bool HasUploadTables;

        public Schema(params Type[] tables)
        {
            this.Tables = new Dictionary<string, SchemaTable>();
            BuildSchema(tables);
            this.HasDownloadTables = this.Tables.Any(x => x.Value.SyncDirection == SyncDirection.Custom || x.Value.SyncDirection == SyncDirection.Download || x.Value.SyncDirection == SyncDirection.TwoWay);
            this.HasUploadTables = this.Tables.Any(x => x.Value.SyncDirection == SyncDirection.Custom || x.Value.SyncDirection == SyncDirection.Upload || x.Value.SyncDirection == SyncDirection.TwoWay);
        }

        public Dictionary<string, SchemaTable> GetAll() => this.Tables;

        private void BuildSchema(params Type[] tables)
        {
            foreach (Type type in tables)
            {
                var typeInfo = type.GetTypeInfo();
#if ENABLE_IL2CPP
			var directionAttribute = typeInfo.GetCustomAttribute<SyncMode> ();
#else
                var directionAttribute =
                    typeInfo.CustomAttributes
                        .Where(x => x.AttributeType == typeof(SyncMode))
                        .Select(x => (SyncMode)x.InflateAttribute())
                        .FirstOrDefault();
#endif
                string tableName = TableMapping.GetTableName(type);
                if (directionAttribute is null)
                {
                    Log.Logger.Warning("SyncDirection is not defined");
                    directionAttribute = new SyncMode(SyncDirection.Download);
                }
                var direction = directionAttribute.Direction;

                this.Tables.TryGetValue(tableName, out SchemaTable mapping);
                if (mapping is null)
                {
                    mapping = new SchemaTable(tableName, direction);
                    this.Tables.Add(tableName, mapping);
                }
                mapping.Add(Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                mapping.Add(Daemon.Current.DaemonConfig.Local.GetMapping(type));
                Log.Logger.Information("BUILDED SCHEMA [{0}] - [{1}]", type.FullName, directionAttribute.Direction);
            }
        }

        public SchemaTable this[string TableName, SyncTarget direcction]
        {
            get
            {
                string key = TableName;
                if (DeniedTables != null && DeniedTables.Contains(key))
                {
                    return null;
                }
                if (this.Tables.ContainsKey(key))
                    return this.Tables[key];
                if (this.DeniedTables is null)
                {
                    this.DeniedTables = new HashSet<string>();
                }
                DeniedTables.Add(key);

                //if (!IsValidDirection(table.TableDirection, direcction))
                //{
                //    return null;
                //}
                return null;
            }
        }

        private bool IsValidDirection(TableDirection TableDirection, SyncTarget UseDirection)
        {
            if (TableDirection == TableDirection.TWO_WAY)
            {
                return true;
            }
            if (TableDirection == TableDirection.UPLOAD && UseDirection == SyncTarget.Remote)
            {
                return true;
            }
            if (TableDirection == TableDirection.DOWNLOAD && UseDirection == SyncTarget.Local)
            {
                return true;
            }
            return false;
        }

        internal bool CheckTriggers(SQLServerConnection Connection)
        {
            foreach (TableMapping map in
                this.Tables
                .Select(d => d.Value.ForSqlServer())
                .Where(x => x.SyncDirection == SyncDirection.Download || x.SyncDirection == SyncDirection.TwoWay))
            {
                Trigger.CheckTrigger(Connection, map, Daemon.Current.DaemonConfig.DbVersion);
                InitTableAttribute.Find(map.MappedType)?.Execute(Connection);
            }
            return true;
        }
    }
}