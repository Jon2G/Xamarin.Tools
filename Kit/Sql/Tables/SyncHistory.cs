using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;


namespace Kit.Sql.Tables
{
    public class SyncHistory:ISync
    {
        [MaxLength(100)]
        public string DeviceId { get; set; }
        public DateTime Date { get; set; }

        public void Save(IDbConnection origin)
        {
            throw new NotImplementedException();
        }
    }
}
