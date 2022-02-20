using System;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Base;

namespace Kit.Sql.Tables
{
    public class SyncHistory:ISync
    {
        [MaxLength(100)]
        public string DeviceId { get; set; }
        public DateTime Date { get; set; }

        public static void Delete(SqlBase con,Guid syncGuid)
        {
            con.Table<SyncHistory>().Delete(x => x.Guid == syncGuid,false);
        }
    }
}
