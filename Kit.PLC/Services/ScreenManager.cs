using System;
using System.Collections.Generic;
using System.Text;
using Kit.Services.Interfaces;

namespace Kit.Services
{
    public static class ScreenManager
    {
        /// <summary>
        /// Lazy-initialized file picker implementation
        /// </summary>
        private static Lazy<IScreenManager> implementation =
            new Lazy<IScreenManager>(Create, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current file picker plugin implementation to use
        /// </summary>
        public static IScreenManager Current
        {
            get
            {
                var ret = implementation.Value;
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
        private static IScreenManager Create()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0 || NETCOREAPP
            return null;
#else
#if MONOANDROID
            return new Droid.Services.ScreenManagerService();
#endif
#if __IOS__
            return new iOS.Services.ScreenManagerService();
#endif
#if WINDOWS_UWP
            return new UWP.Services.ScreenManagerService();
#endif
#if NET47
            return new WPF.Services.ScreenManagerService();
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
