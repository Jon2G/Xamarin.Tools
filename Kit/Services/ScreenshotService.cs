
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Services.Interfaces;

namespace Kit.Services
{
    public static class ScreenshotService
    {
        /// <summary>
        /// Lazy-initialized file picker implementation
        /// </summary>
        private static Lazy<IScreenshot> implementation =
            new Lazy<IScreenshot>(Create, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current file picker plugin implementation to use
        /// </summary>
        public static IScreenshot Current
        {
            get
            {
                IScreenshot ret = implementation.Value;
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
        private static IScreenshot Create()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#if MONOANDROID
            return new Droid.Services.Screenshot();
#endif
#if __IOS__
            return new iOS.Services.Screenshot();
#endif
#if WINDOWS_UWP
                return new UWP.Services.ScreenShotService();
#endif
#if NET47
            return new WPF.Services.ScreenShotService();
#endif
#if NETCOREAPP
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
