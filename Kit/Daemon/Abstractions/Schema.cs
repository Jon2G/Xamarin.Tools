
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Daemon.Enums;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using SQLServer;
using static Kit.Sql.Base.BaseOrm;

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

                switch (directionAttribute.Direction)
                {
                    case SyncDirection.TwoWay:
                        this.DownloadTables.Add(
                            Daemon.Current.DaemonConfig.Remote.GetTableMappingKey(type)
                            , Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                        this.UploadTables.Add(
                            Daemon.Current.DaemonConfig.Local.GetTableMappingKey(type)
                            , Daemon.Current.DaemonConfig.Local.GetMapping(type));
                        break;
                    case SyncDirection.Download:
                        this.DownloadTables.Add(
                            Daemon.Current.DaemonConfig.Remote.GetTableMappingKey(type)
                            , Daemon.Current.DaemonConfig.Remote.GetMapping(type));
                        break;
                    case SyncDirection.Upload:
                        this.UploadTables.Add(
                            Daemon.Current.DaemonConfig.Local.GetTableMappingKey(type)
                            , Daemon.Current.DaemonConfig.Local.GetMapping(type));
                        break;
                    case SyncDirection.NoSync:
                        Log.Logger.Warning($"La tabla [{type.FullName}] esta definida en el schema pero se marco como no sincronizar");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public Table this[string TableName, SyncDirecction direcction]
        {
            get
            {
                if (DeniedTables != null && DeniedTables.Contains(TableName))
                {
                    return null;
                }

                Table table = null; //this.Tables.FirstOrDefault(x => string.Compare(x.Key, TableName, true) == 0);
                if (table is null)
                {
                    if (this.DeniedTables is null)
                    {
                        this.DeniedTables = new HashSet<string>();
                    }
                    DeniedTables.Add(TableName);
                }
                if (!IsValidDirection(table.TableDirection, direcction))
                {
                    return null;
                }
                return table;
            }
        }
        private bool IsValidDirection(TableDirection TableDirection, SyncDirecction UseDirection)
        {
            if (TableDirection == TableDirection.TWO_WAY)
            {
                return true;
            }
            if (TableDirection == TableDirection.UPLOAD && UseDirection == SyncDirecction.Remote)
            {
                return true;
            }
            if (TableDirection == TableDirection.DOWNLOAD && UseDirection == SyncDirecction.Local)
            {
                return true;
            }
            return false;
        }

        internal bool CheckTriggers(SQLServerConnection Connection)
        {
            foreach (var table in this.DownloadTables)
            {
                //if (IsSleepRequested || OffLine)
                //{
                //    Start();
                //    return;
                //}
                Trigger.CheckTrigger(Connection, table.Value,Daemon.Current.DaemonConfig.DbVersion);
            }

            return true;
        }
    }
}
