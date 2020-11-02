using Acr.UserDialogs.Infrastructure;
using Android.OS;
using Com.Xamarin.Formsviewgroup;
using DeviceId;
using Plugin.DeviceInfo.Abstractions;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Android.Provider.Settings;
using Platform = Plugin.DeviceInfo.Abstractions.Platform;
using IDeviceInfo = Kit.Services.Interfaces.IDeviceInfo;

namespace Kit.Droid.Services
{
    public class DeviceInfo : IDeviceInfo
    {
        public string MacAdress
        {
            get
            {
                DeviceIdBuilder builder = new DeviceIdBuilder();
                builder.AddMacAddress();
                return builder.ToString();
            }
        }
        /// <summary>
        /// Returns the build serial wich is unique foreach device
        /// </summary>
        public string DeviceId
        {
            get
            {
                string id = null;
                try
                {
                    var context = Android.App.Application.Context;
                    id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
                }
                if (string.IsNullOrEmpty(id))
                {
                    DeviceIdBuilder builder = new DeviceIdBuilder();
                    builder.AddProcessorId();
                    return builder.ToString();
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
