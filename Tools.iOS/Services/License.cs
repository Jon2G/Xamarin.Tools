using System;
using System.Collections.Generic;
using System.Text;
using Tools.Forms.Pages;
using Tools.iOS.Services;
using BaseLicense = Tools.License.Licence;
namespace Tools.iOS.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(new CustomMessageBoxService(), new DeviceInfo(), AppName)
        {

        }

        protected override async void OpenRegisterForm()
        {
            var page = Xamarin.Forms.Application.Current.MainPage;

            DeviceRegister login = new DeviceRegister(page.Background, this,new CustomMessageBoxService());
            await page.Navigation.PushModalAsync(login, true);
        }
    }
}
