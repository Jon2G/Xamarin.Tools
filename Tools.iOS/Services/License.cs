using System;
using System.Collections.Generic;
using System.Text;
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

            Forms.Blumitech.BlumLogin login = new Forms.Blumitech.BlumLogin(page.Background, this) as Forms.Blumitech.BlumLogin;
            await page.Navigation.PushModalAsync(login, true);
        }
    }
}
