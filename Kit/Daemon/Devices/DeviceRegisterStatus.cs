using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.Daemon.Devices
{
    [StoreAsText]
    public enum DeviceRegisterStatus
    {
        Unkown = -1, Registered = 0, NotRegistered = 1
    }
}
