using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Kit.Sql.Helpers;

namespace Kit.Daemon.Devices
{
    public class Device
    {
        public static Device Current { get; set; }

        static Device()
        {
            Current = new Device(Kit.Tools.Instance.DeviceId);
        }

        public string DeviceId { get; set; }
        public DeviceRegisterStatus RegisterStatus;

        public Device(string DeviceId)
        {
            this.DeviceId = DeviceId;
            this.RegisterStatus = DeviceRegisterStatus.Unkown;
        }

        private bool IsDeviceRegistered(SqlServer SQLH)
        {
            bool registered = SQLH.Exists("SELECT ID_DISPOSITIVO FROM DISPOSITVOS_TABLETS WHERE ID_DISPOSITIVO=@ID_DISPOSITIVO"
                , false, new SqlParameter("ID_DISPOSITIVO", DeviceId));
            this.RegisterStatus = registered ? DeviceRegisterStatus.Registered : DeviceRegisterStatus.NotRegistered;
            return registered;
        }

        public bool EnsureDeviceIsRegistred(SqlServer SQLH)
        {
            //SQL SERVER
            if (!IsDeviceRegistered(SQLH))
            {
                SQLH.EXEC("INSERT INTO DISPOSITVOS_TABLETS(ID_DISPOSITIVO,ULTIMA_CONEXION) VALUES (@ID_DISPOSITIVO,GETDATE())"
                    , CommandType.Text, false, new SqlParameter("ID_DISPOSITIVO", DeviceId));
            }
            else
            {
                SQLH.EXEC("UPDATE DISPOSITVOS_TABLETS SET ULTIMA_CONEXION=GETDATE() WHERE ID_DISPOSITIVO=@ID_DISPOSITIVO"
                    , CommandType.Text, false, new SqlParameter("ID_DISPOSITIVO", DeviceId));
            }
            return IsDeviceRegistered(SQLH);
        }
    }
}
