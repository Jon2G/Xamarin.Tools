
using Plugin.Connectivity;
using Plugin.Xamarin.Tools.Shared.Services;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Blumitech
{
    public sealed class Licence
    {
        private readonly Dictionary<string, string> AppKeys;
        private readonly WebService WebService;
        private readonly string AppName;
        private string AppKey;
        private Licence(string DeviceId, string AppName)
        {
            this.AppName = AppName;
            this.AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"},
                { "MyGourmetPOS","MGPOS2020"}
            };
            this.WebService = new WebService(DeviceId);
        }
        public static Licence Instance(AbstractTools instance, string AppName)
        {
            return new Licence(Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.DeviceId, AppName);
        }
        private string GetDeviceBrand()
        {
            string brand = "GENERIC";
            try
            {
                var Info = Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current;
                brand = $"{Info.DeviceName}@{Info.Model}@{Info.Manufacturer}";
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return brand;
        }
        private string GetDevicePlatform()
        {
            string brand = "GENERIC";
            try
            {
                return Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.Platform.ToString();
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return brand;
        }
        private bool DoIHaveInternet()
        {
            if (!CrossConnectivity.IsSupported)
                return true;

            return CrossConnectivity.Current.IsConnected;
        }
        public async Task<bool> IsAuthorizated(Page page)
        {
            if (Plugin.Xamarin.Tools.Shared.Tools.Instance.Debugging 
                //&& !Services.DeviceInfo.Current.IsDevice
                )
            {
                return true;
            }
            bool Autorized = false;
            ProjectActivationState state = ProjectActivationState.Unknown;
            if (!DoIHaveInternet())
            {
                state = ProjectActivationState.ConnectionFailed;
            }
            else
            {
                state = await Autheticate(this.AppName);
            }
            switch (state)
            {
                case ProjectActivationState.Active:
                    Log.LogMe("Project is active");
                    Autorized = true;
                    break;
                case ProjectActivationState.Expired:
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("La licencia para usar esta aplicación ha expirado", "Acceso denegado");
                    break;
                case ProjectActivationState.Denied:
                    Log.LogMe("Acces denied");
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Este dispositivo no cuenta con la licencia para usar esta aplicación", "Acceso denegado");
                    break;
                case ProjectActivationState.LoginRequired:
                    Acr.UserDialogs.UserDialogs.Instance.Alert("Este dispositivo debe ser registrado con una licencia valida antes de poder acceder a la aplicación", "Acceso denegado");
                    BlumLogin login = new BlumLogin(page.Background, this).LockModal() as BlumLogin;
                    await page.Navigation.PushModalAsync(login,true);
                    Autorized = false;
                    break;
                case ProjectActivationState.ConnectionFailed:
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Revise su conexión a internet", "Atención");
                    break;
            }
            return Autorized;
        }

        internal async Task<bool> RegisterDevice(string userName, string password)
        {
            string response = await this.WebService.DevicesLeft(this.AppKey, userName);
            switch (response)
            {
                case "ERROR":
                case "INVALID_REQUEST":
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Revise su conexión a internet", "Atención");
                    return false;
                case "CUSTOMER_NOT_FOUND":
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Registro invalido", "Atención");
                    return false;
                case "PROJECT_NOT_FOUND":
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No esta contratado este servicio", "Atención");
                    return false;
            }
            if (!int.TryParse(response, out int left))
            {
                return false;
            }
            if (left <= 0)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No le quedan mas dispositivos para este proyecto", "Atención", "Ok");
                return false;
            }
            else
            {
                string DeviceBrand = this.GetDeviceBrand();
                string Platform = this.GetDevicePlatform();
                switch (await this.WebService.Enroll(AppKey, userName, password, DeviceBrand, Platform))
                {
                    case "NO_DEVICES_LEFT":
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No le quedan mas dispositivos para este proyecto", "Atención", "Ok");
                        break;
                    case "PROJECT_NOT_ENROLLED":
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No esta contratado este servicio", "Atención");
                        break;
                    case "SUCCES":
                        left--;
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync($"Registro exitoso, le quedan: {left} dispositivos", "Atención");
                        return true;
                }
            }
            return false;
        }


        internal async Task<bool> Login(string UserName, string Password)
        {
            return (await this.WebService.LogIn(UserName, Password) == "OK");
        }

        private async Task<ProjectActivationState> Autheticate(string AppKey)
        {
            if (!this.AppKeys.ContainsKey(AppKey))
            {
                return ProjectActivationState.Denied;
            }
            this.AppKey = this.AppKeys[AppKey];
            return await this.WebService.RequestProjectAccess(this.AppKey);
        }


    }
}
