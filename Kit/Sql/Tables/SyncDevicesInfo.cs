using System;
using System.ComponentModel.DataAnnotations;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kit.Sql.Tables
{
    [SyncMode(SyncDirection.NoSync)]
    public class SyncDevicesInfo
    {
        
        [Key, Index(IsClustered = true,IsUnique = true)]
        public int Id { get; set; }
        [Index, MaxLength(100)]
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public DateTime LastTimeSeen { get; set; }

    }
}
