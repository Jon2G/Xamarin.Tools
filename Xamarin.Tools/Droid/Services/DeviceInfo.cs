using Acr.UserDialogs.Infrastructure;
using Android.OS;
using Plugin.DeviceInfo.Abstractions;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using static Android.Provider.Settings;

namespace Plugin.Xamarin.Tools.Droid.Services
{
    public class DeviceInfo : Plugin.Xamarin.Tools.Shared.Services.IDeviceInfo
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
        /// Returns the build serial wich is unique foreach device
        /// </summary>
        public string DeviceId
        {
            get
            {
                string id = Android.OS.Build.GetSerial();
                if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
                {
                    try
                    {
                        var context = Android.App.Application.Context;
                        id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
                    }
                    catch (Exception ex)
                    {
                        Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
                    }
                }
                return id;
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
