using Foundation;
using UIKit;

namespace Kit.MAUI.Services
{
    public class NativeSettingsService : INativeSettingsService
    {
        public void OpenAppSettings()
        {
            string app_bundle_id = AppInfo.PackageName;
            var url = new NSUrl($"app-settings:{app_bundle_id}");
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}
