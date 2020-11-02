using AdSupport;
using Plugin.DeviceInfo;
using Plugin.DeviceInfo.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UIKit;
using Xamarin.Forms.PlatformConfiguration;
using IDeviceInfo = Kit.Services.Interfaces.IDeviceInfo;

namespace Kit.iOS.Services
{
    public class DeviceInfo : IDeviceInfo
    {
        public string MacAdress
        {
            get
            {
                try
                {
                    NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()
                        .OrderBy(intf => intf.NetworkInterfaceType)
                        .FirstOrDefault(intf => intf.OperationalStatus == OperationalStatus.Up
                        && (intf.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                        || intf.NetworkInterfaceType == NetworkInterfaceType.Ethernet));
                    PhysicalAddress hw = ni.GetPhysicalAddress();
                    return string.Join(":", (from ma in hw.GetAddressBytes() select ma.ToString("X2")).ToArray());
                }
                catch (Exception ex)
                {
                    SQLHelper.Log.LogMe(ex, "Trying to get the MacAdress");
                    return "Unavaible";
                }
            }
        }
        /// <summary>
        /// Returns the IdentifierForVendor wich is unique for each device
        /// </summary>
        public string DeviceId
        {
            get
            {
                return UIDevice.CurrentDevice.IdentifierForVendor.AsString();
            }
        }

        public string Id => Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;

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
