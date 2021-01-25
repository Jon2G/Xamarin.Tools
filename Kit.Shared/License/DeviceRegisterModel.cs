using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;

namespace Kit.License
{
    public sealed class DeviceRegisterModel : ViewModelBase<DeviceRegisterModel>
    {
        private string _UserName;
        private string _Password;
        public bool IsValidated { get; set; }
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }
        public License Licence { get; set; }
        private readonly ICustomMessageBox CustomMessageBox;
        public DeviceRegisterModel(License licence, ICustomMessageBox CustomMessageBox)
        {
            this.CustomMessageBox = CustomMessageBox;
            this.Licence = licence;
        }

        public async Task<bool> LogIn()
        {
            await Task.Yield();
            IsValidated = !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            if (!IsValidated)
            {
                await CustomMessageBox.ShowOK("Debe llenar todos los campos correctamente", "Alerta", "OK");
                return false;
            }
            if (await this.Licence.Login(UserName, Password))
            {
                if (await Licence.RegisterDevice(UserName, Password))
                {
                    return true;
                }
            }
            else
            {
               await CustomMessageBox.ShowOK("Usuario o contraseña incorrectos", "Alerta", "OK");
            }
            return false;
        }
    }
}
