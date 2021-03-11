﻿using System;
using System.Collections.Generic;
using System.Text;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Base;

namespace Kit.Sql.Tables
{
    public class SyncHistory:ISync
    {
        //[PrimaryKey]
        public Guid SyncGuid { get; set; }
        [MaxLength(100)]
        public string DeviceId { get; set; }

        public void Save(SqlBase origin)
        {
            throw new NotImplementedException();
        }
    }
}
