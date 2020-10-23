using System;
using System.Collections.Generic;
using System.Text;
using BaseLicense = Tools.License.Licence;
namespace Tools.WPF.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(new ICustomMessageBox.CustomMessageBoxService(), new DeviceInfo(), AppName)
        {

        }

        protected override async void OpenRegisterForm()
        {
            throw new NotImplementedException();
        }
    }
}
