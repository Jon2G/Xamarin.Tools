using System;
using Kit.Forms.Services.Interfaces;

namespace Kit.Forms.Services
{
    public class CameraService
    {
        /// <summary>
        /// Lazy-initialized file picker implementation
        /// </summary>
        private static Lazy<ICameraService> implementation =
            new Lazy<ICameraService>(CreateDataShare, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current file picker plugin implementation to use
        /// </summary>
        public static ICameraService Current
        {
            get
            {
                ICameraService ret = implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }

                return ret;
            }
        }

        /// <summary>
        /// Creates file picker instance for the platform
        /// </summary>
        /// <returns>file picker instance</returns>
        private static ICameraService CreateDataShare()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_1||NETCOREAPP|| NETSTANDARD
            return null;
#else
#if MONOANDROID
            return new Kit.Services.Interfaces.ICameraService();
#endif
#if __IOS__
            return new Kit.Services.Interfaces.ICameraService();
#endif
#if WINDOWS_UWP
            return new Kit.Services.Interfaces.ICameraService();
#endif
#if NET47
            return null;
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
