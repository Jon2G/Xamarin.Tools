using System.Threading.Tasks;
using Kit.Services.Interfaces;
using Kit.WPF.Pages;
using BaseLicense = BlumAPI.License;

namespace Kit.WPF.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(AppName)
        {
        }

        protected override async Task OpenRegisterForm()
        {
            ICrossWindow register = new DeviceRegister(this)
            {
                Owner = null,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };
            await register.ShowDialog();
        }
    }
}