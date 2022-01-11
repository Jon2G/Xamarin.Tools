using System.Threading.Tasks;
using BaseLicense = BlumAPI.License;

namespace Kit.MAUI.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(AppName)
        {
        }

        protected override async Task OpenRegisterForm()
        {
            Kit.MAUI.Pages.DeviceRegister login = new(this, Kit.Tools.Instance.Dialogs);
            await login.ShowDialog();
        }
    }
}