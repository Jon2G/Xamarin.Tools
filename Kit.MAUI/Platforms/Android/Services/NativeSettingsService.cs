using Android.Content;
using Application = Android.App.Application;
using Net = Android.Net;
using Provider = Android.Provider;
namespace Kit.MAUI.Services
{
    public class NativeSettingsService : INativeSettingsService
    {
        public void OpenAppSettings()
        {
            string package_name = AppInfo.PackageName;
            var intent = new Intent(Provider.Settings.ActionApplicationDetailsSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            var uri = Net.Uri.FromParts("package", package_name, null);
            intent.SetData(uri);
            Application.Context.StartActivity(intent);
        }
    }
}
