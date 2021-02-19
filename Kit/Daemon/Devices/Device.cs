using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Kit.Services.Interfaces;
using Kit.Sql.Attributes;
using Kit.Sql.Helpers;
using SQLServer;

namespace Kit.Daemon.Devices
{
    [Table(name: "SYNC_DEVICES")]
    public class Device
    {
        public static Device Current { get; set; }
        public IDeviceInfo IDeviceInfo { get; private set; }

        public string DeviceId { get; set; }
        public DeviceRegisterStatus RegisterStatus;

        public Device(IDeviceInfo IDeviceInfo)
        {
            this.IDeviceInfo = IDeviceInfo;
            this.DeviceId = IDeviceInfo.DeviceId;
            this.RegisterStatus = DeviceRegisterStatus.Unkown;
        }
        internal static void Init(IDeviceInfo IDeviceInfo)
        {
            Current = new Device(IDeviceInfo);
        }

        private bool IsDeviceRegistered(SQLServerConnection SQLH)
        {
            bool registered = SQLH.Exists("SELECT ID_DISPOSITIVO FROM DISPOSITVOS_TABLETS WHERE ID_DISPOSITIVO=@ID_DISPOSITIVO"
                , new SqlParameter("ID_DISPOSITIVO", DeviceId));
            this.RegisterStatus = registered ? DeviceRegisterStatus.Registered : DeviceRegisterStatus.NotRegistered;
            return registered;
        }

        public bool EnsureDeviceIsRegistred(SQLServerConnection SQLH)
        {
            //SQL SERVER
            if (!IsDeviceRegistered(SQLH))
            {
                SQLH.Exists("INSERT INTO DISPOSITVOS_TABLETS(ID_DISPOSITIVO,ULTIMA_CONEXION) VALUES (@ID_DISPOSITIVO,GETDATE())"
                    , new SqlParameter("ID_DISPOSITIVO", DeviceId));
            }
            else
            {
                SQLH.Exists("UPDATE DISPOSITVOS_TABLETS SET ULTIMA_CONEXION=GETDATE() WHERE ID_DISPOSITIVO=@ID_DISPOSITIVO"
                    , new SqlParameter("ID_DISPOSITIVO", DeviceId));
            }
            return IsDeviceRegistered(SQLH);
        }
    }
}
