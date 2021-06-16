using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public readonly Dictionary<string, TableMapping> UploadTables;
        public readonly Dictionary<string, TableMapping> DownloadTables;
        private HashSet<string> DeniedTables;
        public readonly bool HasDownloadTables;
        public readonly bool HasUploadTables;

        public Schema(params Type[] tables)
        {
            this.UploadTables = new Dictionary<string, TableMapping>();
            this.DownloadTables = new Dictionary<string, TableMapping>();
            BuildSchema(tables);
            this.HasDownloadTables = this.DownloadTables.Any();
            this.HasUploadTables = this.UploadTables.Any();
        }

        public Dictionary<string, TableMapping> GetAll() => this.DownloadTables.Merge(this.UploadTables);

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
                        .Select(x => (SyncMode)InflateAttribute(x))
                        .FirstOrDefault();
#endif
                if (directionAttribute is null)
                {
                    Log.Logger.Warning("SyncDirection is not defined");
                    directionAttribute = new SyncMode(SyncDirection.Download);
                }

                if (directionAttribute.Direction == SyncDirection.Download || directionAttribute.Direction == SyncDirection.TwoWay)
                {
                    this.DownloadTables.Add(
                        Daemon.Current.DaemonConfig.Remote.GetTableMappingKey(TableMapping.GetTableName(type))
                        , Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                    this.DownloadTables.Add(
                        Daemon.Current.DaemonConfig.Local.GetTableMappingKey(TableMapping.GetTableName(type))
                        , Daemon.Current.DaemonConfig.Local.GetMapping(type));
                }

                if (directionAttribute.Direction == SyncDirection.Upload || directionAttribute.Direction == SyncDirection.TwoWay)
                {
                    this.UploadTables.Add(
                    Daemon.Current.DaemonConfig.Remote.GetTableMappingKey(TableMapping.GetTableName(type))
                    , Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                    this.UploadTables.Add(
                        Daemon.Current.DaemonConfig.Local.GetTableMappingKey(TableMapping.GetTableName(type))
                        , Daemon.Current.DaemonConfig.Local.GetMapping(type));
                }

                //switch (directionAttribute.Direction)
                //{
                //    case SyncDirection.TwoWay:
                //        this.DownloadTables.Add(
                //            Daemon.Current.DaemonConfig.Local.GetTableMappingKey(TableMapping.GetTableName(type))
                //            , Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                //        this.UploadTables.Add(
                //            Daemon.Current.DaemonConfig.Local.GetTableMappingKey(TableMapping.GetTableName(type))
                //            , Daemon.Current.DaemonConfig.Local.GetMapping(type));
                //        break;
                //    case SyncDirection.Download:
                //        this.DownloadTables.Add(
                //            Daemon.Current.DaemonConfig.Local.GetTableMappingKey(TableMapping.GetTableName(type))
                //            , Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                //        break;
                //    case SyncDirection.Upload:
                //        this.UploadTables.Add(
                //            Daemon.Current.DaemonConfig.Local.GetTableMappingKey(TableMapping.GetTableName(type))
                //            , Daemon.Current.DaemonConfig.Local.GetMapping(type));
                //        break;
                //    case SyncDirection.NoSync:
                //        Log.Logger.Warning($"La tabla [{type.FullName}] esta definida en el schema pero se marco como no sincronizar");
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}
                Log.Logger.Information("BUILDED SCHEMA [{0}] - [{1}]", type.FullName, directionAttribute.Direction);
            }
        }

        public TableMapping this[string TableName, SyncTarget direcction]
        {
            get
            {
                if (DeniedTables != null && DeniedTables.Contains(TableName))
                {
                    return null;
                }

                string key = Daemon.Current.DaemonConfig.Remote.GetTableMappingKey(TableName);
                switch (direcction)
                {
                    case SyncTarget.Local:
                        if (this.DownloadTables.ContainsKey(key))
                            return this.DownloadTables[key];
                        break;

                    case SyncTarget.Remote:
                        if (this.DownloadTables.ContainsKey(key))
                            return this.UploadTables[key];
                        break;
                }
                if (this.DeniedTables is null)
                {
                    this.DeniedTables = new HashSet<string>();
                }
                DeniedTables.Add(TableName);

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
            foreach (var table in this.DownloadTables.Where(x => x.Value is Kit.Sql.SqlServer.TableMapping))
            {
                //if (IsSleepRequested || OffLine)
                //{
                //    Start();
                //    return;
                //}
                Trigger.CheckTrigger(Connection, table.Value, Daemon.Current.DaemonConfig.DbVersion);
            }

            return true;
        }
    }
}