
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Tools.Data;
using Tools.Enums;
using Tools.Services.Interfaces;
using SQLHelper;

namespace Tools.License
{
    public abstract class Licence
    {
        private readonly Dictionary<string, string> AppKeys;
        private readonly WebService WebService;
        private readonly string AppName;
        private string AppKey;
        private readonly IDeviceInfo DeviceInfo;
        private readonly ICustomMessageBox CustomMessageBox;
        protected abstract void OpenRegisterForm();

        protected Licence(ICustomMessageBox CustomMessageBox, IDeviceInfo DeviceInfo, string AppName)
        {
            this.CustomMessageBox = CustomMessageBox;
            this.DeviceInfo = DeviceInfo;
            this.AppName = AppName;
            AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"},
                { "MyGourmetPOS","MGPOS2020"},
                { "Alta y Modificación de Artículos","ALTA2020"}

            };
            WebService = new WebService(DeviceInfo.DeviceId);
        }

        private string GetDeviceBrand()
        {
            string brand = "GENERIC";
            try
            {
                brand = DeviceInfo.Manufacturer;
                if (brand.ToLower() == "unknown")
                {
                    brand = "GENERIC";
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return brand;
        }
        private string GetDeviceName()
        {
            string DeviceName = "GENERIC";
            try
            {
                DeviceName = DeviceInfo.DeviceName;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return DeviceName;
        }
        private string GetDeviceModel()
        {
            string Model = "GENERIC";
            try
            {
                Model = DeviceInfo.Model;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return Model;
        }
        private string GetDevicePlatform()
        {
            string brand = "GENERIC";
            try
            {
                return DeviceInfo.Platform.ToString();
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return brand;
        }

        public async Task<bool> IsAuthorizated()
        {
            if (Data.Tools.Instance.Debugging
                || !DeviceInfo.IsDevice
                )
            {
                return true;
            }
            bool Autorized = false;
            ProjectActivationState state = ProjectActivationState.Unknown;
            state = await Autheticate(AppName);
            switch (state)
            {
                case ProjectActivationState.Active:
                    Log.LogMe("Project is active");
                    Autorized = true;
                    break;
                case ProjectActivationState.Expired:

                    this.CustomMessageBox.Show("La licencia para usar esta aplicación ha expirado", "Acceso denegado");
                    break;
                case ProjectActivationState.Denied:
                    Log.LogMe("Acces denied");
                    this.CustomMessageBox.Show("Este dispositivo no cuenta con la licencia para usar esta aplicación", "Acceso denegado");
                    break;
                case ProjectActivationState.LoginRequired:
                    this.CustomMessageBox.Show("Este dispositivo debe ser registrado con una licencia valida antes de poder acceder a la aplicación", "Acceso denegado");
                    OpenRegisterForm();
                    //     BlumLogin login = new BlumLogin(page.Background, this) as BlumLogin;
                    //   await page.Navigation.PushModalAsync(login, true);
                    Autorized = false;
                    break;
                case ProjectActivationState.ConnectionFailed:
                    this.CustomMessageBox.Show("Revise su conexión a internet", "Atención");
                    break;
            }
            return Autorized;
        }

        public async Task<bool> RegisterDevice(string userName, string password)
        {
            string response = await WebService.DevicesLeft(AppKey, userName);
            switch (response)
            {
                case "ERROR":
                case "INVALID_REQUEST":


                    this.CustomMessageBox.Show("Revise su conexión a internet", "Atención");


                    return false;
                case "CUSTOMER_NOT_FOUND":

                    this.CustomMessageBox.Show("Registro invalido", "Atención");

                    return false;
                case "PROJECT_NOT_FOUND":

                    this.CustomMessageBox.Show("No esta contratado este servicio", "Atención");

                    return false;
            }
            if (!int.TryParse(response, out int left))
            {
                return false;
            }
            if (left <= 0)
            {

                this.CustomMessageBox.Show("No le quedan mas dispositivos para este proyecto", "Atención");


                return false;
            }
            else
            {
                string DeviceBrand = GetDeviceBrand();
                string Platform = GetDevicePlatform();
                string Name = GetDeviceName();
                string Model = GetDeviceModel();
                switch (await WebService.Enroll(AppKey, userName, password, DeviceBrand, Platform, Name, Model))
                {
                    case "NO_DEVICES_LEFT":

                        this.CustomMessageBox.Show("No le quedan mas dispositivos para este proyecto", "Atención");

                        break;
                    case "PROJECT_NOT_ENROLLED":

                        this.CustomMessageBox.Show("No esta contratado este servicio", "Atención");

                        break;
                    case "SUCCES":
                        left--;
                        this.CustomMessageBox.Show($"Registro exitoso, le quedan: {left} dispositivos", "Atención");

                        return true;
                }
            }
            return false;
        }


        public async Task<bool> Login(string UserName, string Password)
        {
            return await WebService.LogIn(UserName, Password) == "OK";
        }

        private async Task<ProjectActivationState> Autheticate(string AppKey)
        {
            if (!AppKeys.ContainsKey(AppKey))
            {
                return ProjectActivationState.Denied;
            }
            this.AppKey = AppKeys[AppKey];
            return await WebService.RequestProjectAccess(this.AppKey);
        }


    }
}
