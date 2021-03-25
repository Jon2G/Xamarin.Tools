using System;
using System.Collections.Generic;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Sql.Attributes;
using Kit.Sql.Base;

namespace Kit.Sql.Tables
{
    [Table("DEVICE_INFORMATION")]
    public class DeviceInformation
    {
        [PrimaryKey, MaxLength(500)]
        public string DeviceId { get; set; }
        public bool IsFirstLaunchTime { get; set; }
        public DeviceInformation()
        {
            //Is not the first time unless proven otherwise
            this.IsFirstLaunchTime = false;
            this.DeviceId = Daemon.Devices.Device.Current.DeviceId;
        }
        [Column("LAST_TIME_SEEN")]
        public DateTime LastAuthorizedTime { get; set; }

        public DeviceInformation Get(SqlBase sql)
        {
            return sql.Table<Kit.Sql.Tables.DeviceInformation>()
                .FirstOrDefault(x => x.DeviceId == Device.Current.DeviceId) ?? new DeviceInformation();

        }
    }
}
