using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.WPF.Pages;
using BaseLicense = Kit.License.License;
namespace Kit.WPF.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base( AppName)
        {

        }

        protected override Task OpenRegisterForm()
        {
            DeviceRegister register = new DeviceRegister(this, new ICustomMessageBox.CustomMessageBoxService())
            {
                Owner = null,
                WindowStartupLocation=System.Windows.WindowStartupLocation.CenterScreen
            };
            register.ShowDialog();
            return Task.FromResult(true);
        }
    }
}
