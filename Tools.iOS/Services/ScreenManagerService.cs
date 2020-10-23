using Tools.Enums;
using Tools.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Tools.iOS.Services
{
    public class ScreenManagerService : IScreenManager
    {

        public ScreenManagerService()
        {

        }
        public void SetScreenMode(ScreenMode ScreenMode)
        {
            if (Application.Current.MainPage is NavigationPage page)
            {
                switch (ScreenMode)
                {
                    case ScreenMode.Normal:
                        NavigationPage.SetHasNavigationBar(page, true);
                        Xamarin.Forms.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(page, false);
                        break;
                    default:
                        NavigationPage.SetHasNavigationBar(page, false);
                        Xamarin.Forms.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(page, true);
                        break;
                }
            }

        }
    }
}
