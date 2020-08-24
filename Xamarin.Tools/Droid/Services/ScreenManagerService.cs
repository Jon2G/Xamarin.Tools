using Android.App;
using Android.Views;
using Plugin.Xamarin.Tools.Shared.Enums;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Droid.Services
{
    public class ScreenManagerService : IScreenManager
    {
        private readonly Activity Activity;
        public ScreenManagerService()
        {
            this.Activity = (Plugin.Xamarin.Tools.Shared.Tools.Instance as Droid.ToolsImplementation).MainActivity;
        }
        public void SetScreenMode(ScreenMode ScreenMode)
        {

            this.Activity.Window.ClearFlags(WindowManagerFlags.Fullscreen);

            int uiOptions = (int)this.Activity.Window.DecorView.SystemUiVisibility;
            uiOptions |= (int)SystemUiFlags.LowProfile;
            switch (ScreenMode)
            {
                case ScreenMode.FullScreen:
                    this.Activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
                    uiOptions |= (int)SystemUiFlags.Fullscreen;
                    uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
                    uiOptions |= (int)SystemUiFlags.HideNavigation;
                    break;
                case ScreenMode.HideControlsBar:
                    this.Activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
                    break;
                case ScreenMode.Normal:
                    uiOptions |= (int)SystemUiFlags.Visible;
                    break;

            }
            this.Activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }
    }
}
