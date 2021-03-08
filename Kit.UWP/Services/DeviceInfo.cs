using System;
using Plugin.DeviceInfo.Abstractions;
using IDeviceInfo = Kit.Services.Interfaces.IDeviceInfo;

namespace Tools.UWP.Services
{
    public class DeviceInfo : IDeviceInfo
    {
        public string MacAdress
        {
            get
            {
                //DeviceIdBuilder builder = new DeviceIdBuilder();
                //builder.AddMacAddress();
                //return builder.ToString();
                return "NotAvaible";
            }
        }

        public string DeviceId => Id;

        public string Id => Plugin.DeviceInfo.CrossDeviceInfo.Current.Id.Replace("+", "-");

        public string Model => Plugin.DeviceInfo.CrossDeviceInfo.Current.Model;

        public string Manufacturer => Plugin.DeviceInfo.CrossDeviceInfo.Current.Manufacturer;

        public string DeviceName => Plugin.DeviceInfo.CrossDeviceInfo.Current.DeviceName;

        public string Version => Plugin.DeviceInfo.CrossDeviceInfo.Current.Version;

        public Version VersionNumber => Plugin.DeviceInfo.CrossDeviceInfo.Current.VersionNumber;

        public string AppVersion => Plugin.DeviceInfo.CrossDeviceInfo.Current.AppVersion;

        public string AppBuild => Plugin.DeviceInfo.CrossDeviceInfo.Current.AppBuild;

        public Platform Platform => Plugin.DeviceInfo.CrossDeviceInfo.Current.Platform;

        public Idiom Idiom => Plugin.DeviceInfo.CrossDeviceInfo.Current.Idiom;

        public bool IsDevice => Plugin.DeviceInfo.CrossDeviceInfo.Current.IsDevice;

        public string GenerateAppId(bool usingPhoneId = false, string prefix = null, string suffix = null)
        {
            return Plugin.DeviceInfo.CrossDeviceInfo.Current.GenerateAppId(usingPhoneId, prefix, suffix);
        }
    }
}
