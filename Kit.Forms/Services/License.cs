using Xamarin.Forms;
using BaseLicense = Kit.License.License;

namespace Kit.Forms.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(AppName)
        {

        }

        protected override async void OpenRegisterForm()
        {
            Page page = Xamarin.Forms.Application.Current.MainPage;

            Forms.Pages.DeviceRegister login = new Forms.Pages.DeviceRegister(page.Background, this,new CustomMessageBoxService());
            await page.Navigation.PushModalAsync(login, true);
        }
    }
}
