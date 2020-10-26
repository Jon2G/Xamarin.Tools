using System;
using System.Collections.Generic;
using System.Text;
using Tools.NetCore.Pages;
using BaseLicense = Tools.License.Licence;
namespace Tools.NetCore.Services
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
