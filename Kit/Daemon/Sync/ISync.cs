using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.Daemon.Sync
{
    public interface ISync
    {
        [Unique, NotNull]
        public Guid SyncGuid { get; set; }


    }
}
