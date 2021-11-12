﻿using Plugin.DeviceInfo.Abstractions;
using System;
using DeviceId;
using System.IO;

namespace Kit.Services.Interfaces
{
    public class DeviceInfo
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
                //try
                //{
                //    id = HardUniqueId();
                //}
                //catch (Exception ex)
                //{
                //    Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
                //}
                FileInfo keyFile = new FileInfo(Path.Combine(Tools.Instance.LibraryPath, "Key.key"));
                if (keyFile.Exists)
                {
                    id = File.ReadAllText(keyFile.FullName);
                    if (!string.IsNullOrEmpty(id))
                    {
                        return id;
                    }
                }
                DeviceIdBuilder builder = new DeviceIdBuilder();
                builder.AddMachineName();
                builder.AddMacAddress(true);
                id = builder.ToString();
                File.WriteAllText(keyFile.FullName, id);
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
