using Acr.UserDialogs.Infrastructure;
using Android.OS;
using Com.Xamarin.Formsviewgroup;
using DeviceId;
using Plugin.DeviceInfo.Abstractions;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Net.Wifi;
using Android.Provider;
using Android.Telephony;
using Java.Security;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Android.Provider.Settings;
using Platform = Plugin.DeviceInfo.Abstractions.Platform;
using IDeviceInfo = Kit.Services.Interfaces.IDeviceInfo;
using Permissions = Xamarin.Essentials.Permissions;
using AndroidX.Core.Content;

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

        private string HardUniqueId()
        {
            //1 compute IMEI
            TelephonyManager TelephonyMgr = (TelephonyManager)Kit.Droid.ToolsImplementation.Instance.MainActivity.GetSystemService(MainActivity.TelephonyService);
            String m_szImei = string.Empty;
            if (ContextCompat.CheckSelfPermission(Kit.Droid.ToolsImplementation.Instance.MainActivity
                , Manifest.Permission.ReadPhoneState) == Android.Content.PM.Permission.Granted)
            {
                m_szImei = TelephonyMgr.DeviceId; // Requires READ_PHONE_STATE
            }

            //2 compute DEVICE ID
            String m_szDevIDShort = "35" + //we make this look like a valid IMEI
                Build.Board.Length % 10 + Build.Board.Length % 10 +
                Build.CpuAbi.Length % 10 + Build.Device.Length % 10 +
                Build.Display.Length % 10 + Build.Host.Length % 10 +
                Build.Id.Length % 10 + Build.Manufacturer.Length % 10 +
                Build.Model.Length % 10 + Build.Product.Length % 10 +
                Build.Tags.Length % 10 + Build.Type.Length % 10 +
                Build.User.Length % 10; //13 digits
                                        //3 android ID - unreliable
            String m_szAndroidID = Secure.GetString(ToolsImplementation.Instance.MainActivity.ContentResolver, Secure.AndroidId);

            //4 wifi manager, read MAC address - requires  android.permission.ACCESS_WIFI_STATE or comes as null
            String m_szWLANMAC = String.Empty;
            if (ContextCompat.CheckSelfPermission(Kit.Droid.ToolsImplementation.Instance.MainActivity
                , Manifest.Permission.AccessWifiState) == Android.Content.PM.Permission.Granted)
            {
                WifiManager wm =
                    (WifiManager)ToolsImplementation.Instance.MainActivity.GetSystemService(MainActivity.WifiService);
                m_szWLANMAC = wm.ConnectionInfo.MacAddress;
            }

            //5 Bluetooth MAC address  android.permission.BLUETOOTH required
            String m_szBTMAC = string.Empty;
            if (ContextCompat.CheckSelfPermission(Kit.Droid.ToolsImplementation.Instance.MainActivity
                , Manifest.Permission.Bluetooth) == Android.Content.PM.Permission.Granted)
            {
                BluetoothAdapter m_BluetoothAdapter = null; // Local Bluetooth adapter
                m_BluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                m_szBTMAC = m_BluetoothAdapter.Address;
            }

            //6 SUM THE IDs
            Java.Lang.String m_szLongID = new Java.Lang.String(m_szImei + m_szDevIDShort + m_szAndroidID + m_szWLANMAC + m_szBTMAC);
            MessageDigest m = null;
            try
            {
                m = MessageDigest.GetInstance("MD5");
            }
            catch (NoSuchAlgorithmException e)
            {
                Log.Logger.Error(e, "Al obtener el algoritmo MD5");
            }
            m.Update(m_szLongID.GetBytes(), 0, m_szLongID.Length());
            byte[] p_md5Data = m.Digest();

            String m_szUniqueID = String.Empty;
            for (int i = 0; i < p_md5Data.Length; i++)
            {
                int b = (0xFF & p_md5Data[i]);
                // if it is a single digit, make sure it have 0 in front (proper padding)
                if (b <= 0xF) m_szUniqueID += "0";
                // add number to string
                m_szUniqueID += Java.Lang.Integer.ToHexString(b);
            }
            m_szUniqueID = m_szUniqueID.ToUpper();

            //m_tv.setText("Android Unique Device ID\n\n\n\n" +

            //        "IMEI:> " + m_szImei +
            //        "\nDeviceID:> " + m_szDevIDShort +
            //        "\nAndroidID:> " + m_szAndroidID +
            //        "\nWLANMAC:> " + m_szWLANMAC +
            //        "\nBTMAC:> " + m_szBTMAC +
            //        "\n\nUNIQUE ID:>" + m_szUniqueID +


            //"\n\n\n(C) 2011 - PocketMagic.net");
            return m_szUniqueID;
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
                DeviceIdBuilder builder = new DeviceIdBuilder();
                builder.AddProcessorId();
                return string.Concat(id ?? string.Empty, builder.ToString());
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
