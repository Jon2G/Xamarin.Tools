using System;
using System.Collections.Generic;
using System.Text;
using Kit.Services.Interfaces;

namespace Kit
{
    public interface ITools
    {
        ITools Init(IDeviceInfo DeviceInfo, string LogDirectory, bool AlertAfterCritical = false);
        void CriticalAlert(object sender, EventArgs e);
    }
}
