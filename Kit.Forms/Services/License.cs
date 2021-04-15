using System.Threading.Tasks;
using Xamarin.Forms;
using BaseLicense = Kit.License.License;

namespace Kit.Forms.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(AppName)
        {

        }

        protected override async Task OpenRegisterForm()
        {
            Forms.Pages.DeviceRegister login = new Forms.Pages.DeviceRegister(this,new CustomMessageBoxService());
            await login.ShowDialog();
        }
    }
}
