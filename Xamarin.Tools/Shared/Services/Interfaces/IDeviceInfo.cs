using Plugin.DeviceInfo.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared.Services.Interfaces
{
    public interface IDeviceInfo : Plugin.DeviceInfo.Abstractions.IDeviceInfo
    {
        public string MacAdress { get; }
        public string DeviceId { get; }
    }
}
