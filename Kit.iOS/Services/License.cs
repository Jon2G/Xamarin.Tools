using System;
using System.Collections.Generic;
using System.Text;
using Kit.Forms.Pages;
using Kit.iOS.Services;
using Xamarin.Forms;
using BaseLicense = Kit.License.License;
namespace Kit.iOS.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(new CustomMessageBoxService(), new DeviceInfo(), AppName)
        {

        }

        protected override async void OpenRegisterForm()
        {
            Page page = Xamarin.Forms.Application.Current.MainPage;

            DeviceRegister login = new DeviceRegister(page.Background, this,new CustomMessageBoxService());
            await page.Navigation.PushModalAsync(login, true);
        }
    }
}
