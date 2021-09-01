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

        public void Save(SqlBase origin)
        {
            throw new NotImplementedException();
        }
    }
}
