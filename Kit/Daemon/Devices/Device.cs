using System;
using System.Linq;
using Kit.Services.Interfaces;
using Kit.Sql.Attributes;
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
        public string GetDeviceBrand()
        {
            string brand = "GENERIC";
            try
            {
                brand = Device.Current.IDeviceInfo.Manufacturer;
                if (brand.ToLower() == "unknown")
                {
                    brand = "GENERIC";
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device brand");
            }
            return brand;
        }
        public string GetDeviceName()
        {
            string DeviceName = "GENERIC";
            try
            {
                DeviceName = Device.Current.IDeviceInfo.DeviceName;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device name");
            }
            return DeviceName;
        }
        public string GetDeviceModel()
        {
            string Model = "GENERIC";
            try
            {
                Model = Device.Current.IDeviceInfo.Model;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device model");
            }
            return Model;
        }
        public string GetDevicePlatform()
        {
            string brand = "GENERIC";
            try
            {
                return Device.Current.IDeviceInfo.Platform.ToString();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device platform");
            }
            return brand;
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
