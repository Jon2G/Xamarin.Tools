using System;
using System.Collections.Generic;
using System.Text;
using Kit.Services.Interfaces;
using IDeviceInfo = Kit.Services.Interfaces.IDeviceInfo;

namespace Kit.Services
{
    public static class DeviceInfo
    {
        /// <summary>
        /// Lazy-initialized file picker implementation
        /// </summary>
        private static Lazy<IDeviceInfo> implementation =
            new Lazy<IDeviceInfo>(Create, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current file picker plugin implementation to use
        /// </summary>
        public static IDeviceInfo Current
        {
            get
            {
                var ret = implementation.Value;
                if (ret == null)
                {
#if NETCOREAPP
                    throw new Exception("NotImplementedInReferenceAssembly");
#else
                    throw NotImplementedInReferenceAssembly();
#endif
                }

                return ret;
            }
        }

        /// <summary>
        /// Creates file picker instance for the platform
        /// </summary>
        /// <returns>file picker instance</returns>
        private static IDeviceInfo Create()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#if MONOANDROID
            return new Droid.Services.DeviceInfo();
#endif
#if __IOS__
            return new iOS.Services.DeviceInfo();
#endif
#if WINDOWS_UWP
            return new UWP.Services.DeviceInfo();
#endif
#if NET47
            return new WPF.Services.DeviceInfo();
#endif
#if NETCOREAPP
            return new NetCore.Services.DeviceInfo();
#endif
#endif
        }

        /// <summary>
        /// Returns new exception to throw when implementation is not found. This is the case when
        /// the NuGet package is not added to the platform specific project.
        /// </summary>
        /// <returns>exception to throw</returns>
        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException(
                "This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
}
