
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Services.Interfaces;
using Kit.Sql;

namespace Kit.License
{
    public abstract class License
    {
        public const string LoginSite = "https://ecommerce.blumitech.com.mx/";
        private readonly Dictionary<string, string> AppKeys;
        private readonly WebService WebService;
        private readonly string AppName;
        private string AppKey;

        public string Reason { get; private set; }
        protected abstract void OpenRegisterForm();

        protected License(string AppName)
        {
            this.Reason = "Desconocido...";
            this.AppName = AppName;
            AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"},
                { "EtiquetadorApp","ETIQUETADOR_2020"},
                { "MyGourmetPOS","MGPOS2020"},
                { "Alta y Modificación de Artículos","ALTA2020"}

            };
            WebService = new WebService(Device.Current.DeviceId);
        }

        private string GetDeviceBrand()
        {
            string brand = "GENERIC";
            try
            {
                brand = Device.Current.IDeviceInfo.Manufacturer;
                if (brand.ToLower() == "unknown")
                {
                    brand = "GENERIC";
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex,"Obtaining device brand");
            }
            return brand;
        }
        private string GetDeviceName()
        {
            string DeviceName = "GENERIC";
            try
            {
                DeviceName = Device.Current.IDeviceInfo.DeviceName;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device name");
            }
            return DeviceName;
        }
        private string GetDeviceModel()
        {
            string Model = "GENERIC";
            try
            {
                Model = Device.Current.IDeviceInfo.Model;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device model");
            }
            return Model;
        }
        private string GetDevicePlatform()
        {
            string brand = "GENERIC";
            try
            {
                return Device.Current.IDeviceInfo.Platform.ToString();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Obtaining device platform");
            }
            return brand;
        }

        public async Task<bool> IsAuthorizated()
        {
            if (Tools.Debugging)
            {
                return await Task.FromResult(true); ;
            }
            bool Autorized = false;
            ProjectActivationState state = ProjectActivationState.Unknown;
            state = await Autheticate(AppName);
            switch (state)
            {
                case ProjectActivationState.Active:
                    Log.Logger.Information("Project is active");
                    Autorized = true;
                    break;
                case ProjectActivationState.Expired:
                    this.Reason = "La licencia para usar esta aplicación ha expirado";
                   await Tools.Instance.CustomMessageBox.Show(this.Reason, "Acceso denegado");
                    break;
                case ProjectActivationState.Denied:
                    Log.Logger.Information("Acces denied");
                    this.Reason = "Este dispositivo no cuenta con la licencia para usar esta aplicación";
                    await Tools.Instance.CustomMessageBox.Show(this.Reason, "Acceso denegado");
                    break;
                case ProjectActivationState.LoginRequired:
                    this.Reason = "Este dispositivo debe ser registrado con una licencia valida antes de poder acceder a la aplicación";
                    await Tools.Instance.CustomMessageBox.Show(this.Reason, "Acceso denegado");
                    OpenRegisterForm();
                    //     BlumLogin login = new BlumLogin(page.Background, this) as BlumLogin;
                    //   await page.Navigation.PushModalAsync(login, true);
                    Autorized = false;
                    break;
                case ProjectActivationState.ConnectionFailed:
                    this.Reason = "Revise su conexión a internet";
                    await Tools.Instance.CustomMessageBox.Show(this.Reason, "Atención");
                    break;
            }
            return Autorized;
        }

        public async Task<bool> RegisterDevice(string userName, string password)
        {
            string DeviceBrand = GetDeviceBrand();
            string Platform = GetDevicePlatform();
            string Name = GetDeviceName();
            string Model = GetDeviceModel();
            switch (await WebService.EnrollDevice(DeviceBrand, Platform, Name, Model, AppKey, userName, password))
            {
                case "NO_DEVICES_LEFT":

                    await Tools.Instance.CustomMessageBox.Show("No le quedan mas dispositivos para este proyecto", "Atención");

                    break;
                case "PROJECT_NOT_ENROLLED":

                    await Tools.Instance.CustomMessageBox.Show("No esta contratado este servicio", "Atención");

                    break;
                case "SUCCES":
                    int left = await GetDevicesLeft(AppKey, userName);
                    if (left != -1)
                    {
                        await Tools.Instance.CustomMessageBox.Show($"Registro exitoso, le quedan: {left} dispositivos", "Atención");
                    }
                    else
                    {
                        await Tools.Instance.CustomMessageBox.Show($"Registro exitoso", "Atención");
                    }
                    return true;
            }
            return false;
        }
        private async Task<int> GetDevicesLeft(string AppKey, string UserName)
        {
            string response = await WebService.DevicesLeft(AppKey, UserName);
            switch (response)
            {
                case "ERROR":
                case "INVALID_REQUEST":
                    await Tools.Instance.CustomMessageBox.Show("Revise su conexión a internet", "Atención");
                    return -1;
                case "CUSTOMER_NOT_FOUND":
                    await Tools.Instance.CustomMessageBox.Show("Registro invalido", "Atención");
                    return -1;
                case "PROJECT_NOT_FOUND":
                    await Tools.Instance.CustomMessageBox.Show("No esta contratado este servicio", "Atención");
                    return -1;
            }
            if (int.TryParse(response, out int left))
            {
                return left;
            }
            return -1;
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
