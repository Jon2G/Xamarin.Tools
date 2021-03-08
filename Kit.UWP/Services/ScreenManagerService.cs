using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Kit.Enums;
using Kit.Services.Interfaces;

namespace Tools.UWP.Services
{
    public class ScreenManagerService : IScreenManager
    {

        public ScreenManagerService()
        {

        }
        public void SetScreenMode(ScreenMode ScreenMode)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(null);
            ApplicationView view = ApplicationView.GetForCurrentView();
            switch (ScreenMode)
            {
                case ScreenMode.FullScreen:
                    view.TryEnterFullScreenMode();
                    break;
                case ScreenMode.HideControlsBar:
                    break;
                case ScreenMode.Normal:
                    view.ExitFullScreenMode();
                    break;

            }
        }
    }
}
