using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kit.Daemon.Enums;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Interfaces;
using Kit.Entity;
using Kit.Sql.Tables;
using Microsoft.EntityFrameworkCore;

namespace Kit.Daemon.Sync
{
    public abstract class ISync : ModelBase, IGuid
    {
        public const string SyncGuidColumnName = "SyncGuid";

        //[NotMapped]
        //public Guid SyncGuid { get => Guid; set => Guid = value; }
        /// <summary>
        /// This guid identifies the row where the change is made
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Index(IsClustered = true, IsUnique = true), Column(SyncGuidColumnName)]
        public virtual Guid Guid { get; set; }

        public virtual void Delete(IDbConnection con, IDbConnection targetcon, DbSet<dynamic> map)
        {
            targetcon.Delete(this);
        }
        public virtual bool CustomUpload(IDbConnection con, IDbConnection targetcon, DbSet<dynamic> map)
        {
            return false;
        }

        public virtual bool Affects(IDbConnection con, object PreviousId)
        {
            return false;
        }

        public virtual bool ShouldSync(IDbConnection source_con, IDbConnection target_con)
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
        /// Mandatory for TwoWay Sync
        /// </summary>
        /// <returns></returns>
        public virtual object GetPk()
        {
            return Guid;
        }

        public virtual SyncStatus GetSyncStatus(IDbConnection source_con)
        {
            return GetSyncStatus(source_con, this.Guid);
        }

        public static SyncStatus GetSyncStatus(IDbConnection source_con, Guid guid)
        {
            var history = source_con.Table<SyncHistory>().FirstOrDefault(x => x.Guid == guid);
            if (history is null)
            {
                ChangesHistory changes = source_con.Table<ChangesHistory>().FirstOrDefault(x => x.Guid == guid);
                if (changes is null)
                {
                    //Has been downloaded by Daemon service since daemon downloads dont get inserted here
                    return SyncStatus.Done;
                }
                return SyncStatus.Pending;
            }
            return SyncStatus.Done;
        }

        protected static Guid GetGuid(IDbConnection source_con, ISync obj)
        {
            ISync Queried = (ISync)source_con.GetContext().GetDbSet(obj.GetType()).DbSet.Find(obj.GetPk());
            return Queried.Guid;
        }

        //public static bool IsSynced(IDbConnection source_con, int id)
        //{
        //    Guid guid = GetGuid<T>(source_con, id);
        //    return GetSyncStatus(source_con, guid) == SyncStatus.Done;
        //}



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

        public virtual ISync GetBySyncGuid(IDbConnection con, Guid syncguid)
        {
            var table = con.Table(this.GetType());
            return table.FirstOrDefault(x => (x as ISync).Guid == syncguid);
        }
    }
}