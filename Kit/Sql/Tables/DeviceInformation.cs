using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Kit.Daemon.Devices;
using Kit.Entity;
using Kit.Sql.Attributes;


namespace Kit.Sql.Tables
{
    [Table("DEVICE_INFORMATION")]
    public class DeviceInformation
    {
        [Key, MaxLength(500)]
        public string DeviceId { get; set; }
        public bool IsFirstLaunchTime { get; set; }
        public DeviceInformation()
        {
            //Is not the first time unless proven otherwise
            this.IsFirstLaunchTime = false;
            this.DeviceId = Daemon.Devices.Device.Current?.DeviceId;
        }
        [Column("LAST_TIME_SEEN")]
        public DateTime LastAuthorizedTime { get; set; }

        public DeviceInformation Get(IDbConnection connection)
        {
            return connection.Table<DeviceInformation>()
                   .FirstOrDefault(x => x.DeviceId == Device.Current.DeviceId) ?? new DeviceInformation();

        }
    }
}
