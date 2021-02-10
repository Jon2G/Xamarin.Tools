using System;
using System.Collections.Generic;
using System.Text;
using Kit.Services.Interfaces;

namespace Kit.Services
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
                ICustomMessageBox ret = implementation.Value;
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
#if NETSTANDARD1_0 || NETSTANDARD2_0
return null;
#else
#if NETCOREAPP
            return new Kit.NetCore.Services.ICustomMessageBox.CustomMessageBoxService();
#endif
#if MONOANDROID
            return new Kit.Droid.Services.CustomMessageBoxService();
#endif
#if __IOS__
            return new Kit.iOS.Services.CustomMessageBoxService();
#endif
#if WINDOWS_UWP
            return new Kit.UWP.Services.CustomMessageBoxService();
#endif
#if NET47
            return new Kit.WPF.Services.ICustomMessageBox.CustomMessageBoxService();
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
