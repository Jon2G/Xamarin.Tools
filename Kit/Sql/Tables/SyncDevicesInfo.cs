using System;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;

namespace Kit.Sql.Tables
{
    [SyncMode(SyncDirection.NoSync)]
    public class SyncDevicesInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, MaxLength(100)]
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public DateTime LastTimeSeen { get; set; }

    }
}
