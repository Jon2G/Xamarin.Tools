using System;
using System.Collections.Generic;
using System.Text;
using Kit.NetCore.Pages;
using BaseLicense = Kit.License.Licence;
namespace Kit.NetCore.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(new ICustomMessageBox.CustomMessageBoxService(),new DeviceInfo(),AppName)
        {

        }

        protected override void OpenRegisterForm()
        {
            DeviceRegister register = new DeviceRegister(this, new ICustomMessageBox.CustomMessageBoxService());
            register.ShowDialog();
        }
    }
}
