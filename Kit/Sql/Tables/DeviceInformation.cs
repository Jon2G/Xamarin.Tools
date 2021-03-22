using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.Sql.Tables
{
    [Table("DEVICE_INFORMATION")]
    public class DeviceInformation
    {
        [PrimaryKey,MaxLength(500)]
        public string DeviceId { get; set; }
        public bool IsFirstLaunchTime { get; set; }
        public DeviceInformation()
        {
            //Is not the first time unless proven otherwise
            this.IsFirstLaunchTime = false;
            this.DeviceId = Daemon.Devices.Device.Current.DeviceId;
        }
    }
}
