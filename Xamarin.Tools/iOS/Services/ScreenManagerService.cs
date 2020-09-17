using Plugin.Xamarin.Tools.Shared.Enums;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using x = Xamarin.Forms;

namespace Plugin.Xamarin.Tools.iOS.Services
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
                        x.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(page, false);
                        break;
                    default:
                        NavigationPage.SetHasNavigationBar(page, false);
                        x.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(page, true);
                        break;
                }
            }

        }
    }
}
