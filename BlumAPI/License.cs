using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlumAPI.Enums;
using Kit;
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Sql.Base;
using Kit.Sql.Tables;

namespace BlumAPI
{
    public abstract class License
    {
        public const string LoginSite = "https://ecommerce.blumitech.com.mx/";
        private readonly Dictionary<string, string> AppKeys;
        private readonly APICaller APICaller;
        private DeviceInformation DeviceInformation;
        private readonly string AppName;
        private string AppKey;
        public string Reason { get; private set; }

        protected abstract Task OpenRegisterForm();

        protected License(string AppName)
        {
            this.Reason = "Desconocido...";
            this.AppName = AppName;
            AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"},
                { "EtiquetadorApp","ETIQUETADOR_2020"},
                { "MyGourmetPOS","MGPOS2020"},
                { "Alta y Modificación de Artículos","ALTA2020"},
                { "Toma de pedidos móvil","PEDIDOS19"},
                { "Woocommerce servicio de sincronización","WOOSYNC21" }
            };
            APICaller = new APICaller(Device.Current.DeviceId);
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
                Log.Logger.Error(ex, "Obtaining device brand");
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

        public async Task<bool> IsAuthorizated(SqlBase SqlBase)
        {
            try
            {
                //if (Tools.Debugging)
                //{
                //   return await Task.FromResult(true);
                //}
                await Task.Yield();
                DeviceInformation = DeviceInformation.Get(SqlBase);
                bool Autorized = false;
                ProjectActivationState state = ProjectActivationState.Unknown;
                state = await Autheticate(AppName);
                switch (state)
                {
                    case ProjectActivationState.Active:
                        Log.Logger.Information("Project is active");
                        Autorized = true;
                        DeviceInformation.LastAuthorizedTime = DateTime.Now;
                        SqlBase.InsertOrReplace(DeviceInformation);
                        break;

                    case ProjectActivationState.Expired:
                        this.Reason = "La licencia para usar esta aplicación ha expirado";
                        await Tools.Instance.Dialogs.CustomMessageBox.Show(this.Reason, "Acceso denegado");
                        break;

                    case ProjectActivationState.Denied:
                        Log.Logger.Information("Acces denied");
                        this.Reason = "Este dispositivo no cuenta con la licencia para usar esta aplicación";
                        await Tools.Instance.Dialogs.CustomMessageBox.Show(this.Reason, "Acceso denegado");
                        break;

                    case ProjectActivationState.LoginRequired:
                        this.Reason = "Este dispositivo debe ser registrado con una licencia valida antes de poder acceder a la aplicación";
                        await Tools.Instance.Dialogs.CustomMessageBox.Show(this.Reason, "Acceso denegado");
                        await OpenRegisterForm();
                        Autorized = await IsAuthorizated(SqlBase);
                        break;

                    case ProjectActivationState.ConnectionFailed:
                        this.Reason = "Revise su conexión a internet";
                        await Tools.Instance.Dialogs.CustomMessageBox.Show(this.Reason, "Atención");
                        return CanBeAuthorizedByTime();
                }
                return Autorized;
            }
            catch (Exception ex)
            {
                await Kit.Tools.Instance.Dialogs.CustomMessageBox.Show(ex.Message, "Alerta", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                Log.Logger.Error(ex, "Al comprobar la licencia");
                return false;
            }
        }

        public bool CanBeAuthorizedByTime()
        {
            if (this.DeviceInformation is not null)
            {
                double days = Math.Abs((DateTime.Now - this.DeviceInformation.LastAuthorizedTime).TotalDays);
                return days < 7;
            }
            return false;
        }

        public async Task<bool> RegisterDevice(string userName, string password)
        {
            string DeviceBrand = GetDeviceBrand();
            string Platform = GetDevicePlatform();
            string Name = GetDeviceName();
            string Model = GetDeviceModel();
            switch (await APICaller.EnrollDevice(DeviceBrand, Platform, Name, Model, AppKey, userName, password))
            {
                case "NO_DEVICES_LEFT":

                    await Tools.Instance.Dialogs.CustomMessageBox.Show("No le quedan mas dispositivos para este proyecto", "Atención");

                    break;

                case "PROJECT_NOT_ENROLLED":

                    await Tools.Instance.Dialogs.CustomMessageBox.Show("No esta contratado este servicio", "Atención");

                    break;

                case "SUCCES":
                    int left = await GetDevicesLeft(AppKey, userName);
                    if (left != -1)
                    {
                        await Tools.Instance.Dialogs.CustomMessageBox.Show($"Registro exitoso, le quedan: {left} dispositivos", "Atención");
                    }
                    else
                    {
                        await Tools.Instance.Dialogs.CustomMessageBox.Show($"Registro exitoso", "Atención");
                    }
                    return true;
            }
            return false;
        }

        private async Task<int> GetDevicesLeft(string AppKey, string UserName)
        {
            string response = await APICaller.DevicesLeft(AppKey, UserName);
            switch (response)
            {
                case "ERROR":
                case "INVALID_REQUEST":
                    await Tools.Instance.Dialogs.CustomMessageBox.Show("Revise su conexión a internet", "Atención");
                    return -1;

                case "CUSTOMER_NOT_FOUND":
                    await Tools.Instance.Dialogs.CustomMessageBox.Show("Registro invalido", "Atención");
                    return -1;

                case "PROJECT_NOT_FOUND":
                    await Tools.Instance.Dialogs.CustomMessageBox.Show("No esta contratado este servicio", "Atención");
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
            string reult = await APICaller.LogIn(UserName, Password);
            return reult.Contains("OK");
        }

        private async Task<ProjectActivationState> Autheticate(string AppKey)
        {
            if (!AppKeys.ContainsKey(AppKey))
            {
                return ProjectActivationState.Denied;
            }
            this.AppKey = AppKeys[AppKey];
            return await APICaller.RequestProjectAccess(this.AppKey);
        }
    }
}