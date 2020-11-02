using Plugin.DeviceInfo.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Services.Interfaces
{
    public interface IDeviceInfo : Plugin.DeviceInfo.Abstractions.IDeviceInfo
    {
        public string MacAdress { get; }
        public string DeviceId { get; }
    }
}
