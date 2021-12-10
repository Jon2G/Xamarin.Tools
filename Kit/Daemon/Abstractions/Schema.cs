using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Kit.Daemon.Enums;
using Kit.Entity;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ITableMapping = System.Data.ITableMapping;


namespace Kit.Daemon.Abstractions
{
    [Preserve(AllMembers = true)]
    public class Schema
    {
        public readonly HashSet<Type> UploadTables;
        public readonly HashSet<Type> DownloadTables;
        public readonly HashSet<Type> AllTables;
        public readonly bool HasDownloadTables;
        public readonly bool HasUploadTables;
        public Type this[string name] => this.AllTables.FirstOrDefault(x => x.Name == name);
        public Schema(params Type[] tables)
        {
            this.UploadTables = new();
            this.DownloadTables = new();
            this.AllTables = new();
            BuildSchema(tables);
            this.HasDownloadTables = this.DownloadTables.Any();
            this.HasUploadTables = this.UploadTables.Any();
        }


        private void BuildSchema(params Type[] tables)
        {
            foreach (Type type in tables)
            {
                var directionAttribute = SyncMode.GetSyncMode(type);
                if (directionAttribute is null)
                {
                    Log.Logger.Warning("SyncDirection is not defined");
                    directionAttribute = new SyncMode(SyncDirection.Download);
                }

                if (directionAttribute.Direction == SyncDirection.Download || directionAttribute.Direction == SyncDirection.TwoWay)
                {
                    if (!AllTables.Contains(type))
                        this.AllTables.Add(type);
                    this.DownloadTables.Add(type);
                }

                if (directionAttribute.Direction == SyncDirection.Upload || directionAttribute.Direction == SyncDirection.TwoWay)
                {
                    if (!AllTables.Contains(type))
                        this.AllTables.Add(type);
                    this.UploadTables.Add(type);
                }
                Log.Logger.Information("BUILDED SCHEMA [{0}] - [{1}]", type.FullName, directionAttribute.Direction);
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

        internal bool CheckTriggers(IDbConnection Connection)
        {
            var InitTableAttributetype = typeof(InitTableAttribute);
            foreach (var table in this.DownloadTables)
            {
                Trigger.CheckTrigger(Connection, table, Daemon.Current.DaemonConfig.DbVersion);
                var InitMethod = table.GetMethods()
                      .Where(m => m.GetCustomAttributes(InitTableAttributetype, false).Any()).FirstOrDefault();
                if (InitMethod is not null && !InitMethod.IsStatic)
                {
                    throw new Exception($"Init table method must be static at {table}");
                }
                InitMethod?.Invoke(null, new object[] { Connection });
            }

            return true;
        }
    }
}