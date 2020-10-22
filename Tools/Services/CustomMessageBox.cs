using System;
using System.Collections.Generic;
using System.Text;
using Tools.Services.Interfaces;
using Tools.Services.Interfaces;

namespace Tools.Services
{
    public static class CustomMessageBox
    {
        /// <summary>
        /// Lazy-initialized file picker implementation
        /// </summary>
        private static Lazy<ICustomMessageBox> implementation =
            new Lazy<Interfaces.ICustomMessageBox>(Create, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current file picker plugin implementation to use
        /// </summary>
        public static ICustomMessageBox Current
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
        private static Interfaces.ICustomMessageBox Create()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0 ||NETCOREAPP
            return null;
#else
#if MONOANDROID
            return new Tools.Droid.Services.CustomMessageBoxService();
#endif
#if __IOS__
            return new iOS.Services.CustomMessageBoxService();
#endif
#if WINDOWS_UWP
            return new UWP.Services.CustomMessageBoxService();
#endif
#if NET462
            return new WPF.Services.ICustomMessageBox.CustomMessageBoxService();
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
