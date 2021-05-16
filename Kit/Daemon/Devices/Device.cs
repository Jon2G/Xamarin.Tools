using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Kit.Services.Interfaces;
using Kit.Sql.Attributes;
using Kit.Sql.Helpers;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;

namespace Kit.Daemon.Devices
{
    [Table(name: "SYNC_DEVICES")]
    public class Device
    {
        public static Device Current { get; set; }
        public DeviceInfo IDeviceInfo { get; private set; }

        public string DeviceId { get; set; }
        public DeviceRegisterStatus RegisterStatus;

        public Device()
        {
            this.IDeviceInfo = new DeviceInfo();
            this.DeviceId = IDeviceInfo.DeviceId;
            this.RegisterStatus = DeviceRegisterStatus.Unkown;
        }
        internal static void Init()
        {
            Current = new Device();
        }

        private void EnsureTableExists(SQLServerConnection SQLH)
        {
            if (!SQLH.TableExists<SyncDevicesInfo>())
            {
                SQLH.CreateTable<SyncDevicesInfo>();
            }
        }
        private bool IsDeviceRegistered(SQLServerConnection SQLH)
        {
            EnsureTableExists(SQLH);
            bool registered = SQLH.Table<SyncDevicesInfo>().Any(x => x.DeviceId == DeviceId);
            this.RegisterStatus = registered ? DeviceRegisterStatus.Registered : DeviceRegisterStatus.NotRegistered;
            return registered;
        }

        public bool EnsureDeviceIsRegistred(SQLServerConnection SQLH)
        {
            SyncDevicesInfo deviceInfo;
            //SQL SERVER
            if (!IsDeviceRegistered(SQLH))
            {
                deviceInfo = new SyncDevicesInfo()
                {
                    DeviceId = DeviceId,
                    LastTimeSeen = DateTime.Now,
                    Name = IDeviceInfo.DeviceName
                };
                SQLH.Insert(deviceInfo);
            }
            else
            {
                deviceInfo= SQLH.Table<SyncDevicesInfo>().FirstOrDefault(x => x.DeviceId == DeviceId);
                deviceInfo.LastTimeSeen=DateTime.Now;
                SQLH.Update(deviceInfo);
            }
            return IsDeviceRegistered(SQLH);
        }
    }
}
