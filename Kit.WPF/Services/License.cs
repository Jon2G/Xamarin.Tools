using System;
using System.Collections.Generic;
using System.Text;
using Kit.WPF.Pages;
using BaseLicense = Kit.License.License;
namespace Kit.WPF.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base( AppName)
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
