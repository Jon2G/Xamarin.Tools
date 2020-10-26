using System;
using System.Collections.Generic;
using System.Text;
using Tools.WPF.Pages;
using BaseLicense = Tools.License.Licence;
namespace Tools.WPF.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(new ICustomMessageBox.CustomMessageBoxService(), new DeviceInfo(), AppName)
        {

        }

        protected override void OpenRegisterForm()
        {
            DeviceRegister register = new DeviceRegister(this, new ICustomMessageBox.CustomMessageBoxService())
            {
                Owner = Tools.Instance.VentanaPadre()
            };
            register.ShowDialog();
        }
    }
}
