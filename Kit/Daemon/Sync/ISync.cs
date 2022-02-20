﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kit.Daemon.Enums;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Interfaces;
using Kit.Sql.Sqlite;
using Kit.Sql.Tables;

namespace Kit.Daemon.Sync
{
    public abstract class ISync : ModelBase, IGuid
    {
        public const string SyncGuidColumnName = "SyncGuid";

        //[Ignore]
        //public Guid SyncGuid { get => Guid; set => Guid = value; }
        /// <summary>
        /// This guid identifies the row where the change is made
        /// </summary>
        [AutoIncrement, Column(SyncGuidColumnName)]
        public virtual Guid Guid { get; set; }
        public virtual SyncStatus SyncStatus { get; protected set; }
        public virtual void Delete(SqlBase con, SqlBase targetcon, Kit.Sql.Base.TableMapping map)
        {
            targetcon.Delete(this);
        }
        public virtual bool CustomUpload(SqlBase con, SqlBase targetcon, Kit.Sql.Base.TableMapping map)
        {
            return false;
        }

        public virtual bool Affects(SyncManager syncManager,Kit.Sql.Sqlite.SQLiteConnection con, object PreviousId)
        {
            return false;
        }

        public virtual bool ShouldSync(SqlBase source_con, SqlBase target_con)
        {
            return true;
        }

        public virtual void OnDownloaded(NotifyTableChangedAction action)
        {
        }

        public virtual void OnUploaded(NotifyTableChangedAction action)
        {
        }
        /// <summary>
        /// Fires after sql activates instance
        /// </summary>
        /// <param name="sql"></param>
        public virtual void OnLoad(SqlBase sql)
        {

        }
        /// <summary>
        /// Mandatory for TwoWay Sync
        /// </summary>
        /// <returns></returns>
        public virtual object GetPk()
        {
            return Guid;
        }
        public virtual SyncStatus GetSyncStatus(SqlBase source_con)
        {
            this.SyncStatus= GetSyncStatus(source_con, this.Guid, this.SyncStatus);
            return SyncStatus;
        }
        public static SyncStatus GetSyncStatus(SqlBase source_con, Guid guid, SyncStatus SyncStatus)
        {
            var history = source_con.Table<SyncHistory>().FirstOrDefault(x => x.Guid == guid);
            if (history is null)
            {
                ChangesHistory changes = source_con.Table<ChangesHistory>().FirstOrDefault(x => x.Guid == guid);
                if (changes is null)
                {
                    //Has been downloaded by Daemon service since daemon downloads dont get inserted here
                    SyncStatus = SyncStatus.Done;
                    return SyncStatus;
                }
                if (SyncStatus == SyncStatus.Failed)
                {
                    return SyncStatus.Failed;
                }
                SyncStatus = SyncStatus.Pending;
                return SyncStatus.Pending;
            }
            SyncStatus = SyncStatus.Done;
            return SyncStatus.Done;
        }
        protected static Guid GetGuid<T>(SqlBase source_con, object id)
        {
            Guid result = Guid.NewGuid();
            var map = source_con.GetMapping<T>();
            string text = source_con.Single<string>($"SELECT SyncGuid FROM {map.TableName} where {map.PK.Name}='{id}'");
            if (!string.IsNullOrEmpty(text))
            {
                Guid.TryParse(text, out result);
            }
            return result;
        }
        public static bool IsSynced<T>(SqlBase source_con, int id)
        {
            Guid guid = GetGuid<T>(source_con, id);
            return GetSyncStatus(source_con, guid,SyncStatus.Unknown) == SyncStatus.Done;
        }
        internal void OnSynced(SyncTarget direccion, NotifyTableChangedAction action)
        {
            switch (direccion)
            {
                case SyncTarget.Local:
                    OnDownloaded(action);
                    break;

                case SyncTarget.Remote:
                    OnUploaded(action);
                    break;
            }
        }
        public virtual ISync GetBySyncGuid(SqlBase con, Guid syncguid)
        {
            var table = con.Table(this.GetType()).Table;
            string selection_list = table.SelectionList;
            string condition = (con is SQLiteConnection ? "SyncGuid=?" : "SyncGuid=@SyncGuid");
            CommandBase command = con.CreateCommand($"SELECT {selection_list} FROM {table.TableName} WHERE {condition}",
             new BaseTableQuery.Condition("SyncGuid", syncguid));

            MethodInfo method = command.GetType().GetMethod(nameof(CommandBase.ExecuteDeferredQuery), new[] { typeof(Kit.Sql.Base.TableMapping) });
            method = method.MakeGenericMethod(table.MappedType);
            IEnumerable<dynamic> result = (IEnumerable<dynamic>)method.Invoke(command, new object[] { table });

            dynamic i_result = result.FirstOrDefault();
            ISync read = Convert.ChangeType(i_result, typeof(ISync));

            return read;
        }
        public virtual void OnSyncFailed(SqlBase target, SqlBase source, Exception reason)
        {

        }
    }
}