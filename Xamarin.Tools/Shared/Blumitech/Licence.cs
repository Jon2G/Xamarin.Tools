
using Tools.Forms.Blumitech;
using Plugin.Xamarin.Tools.Shared.Services;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Xamarin.Tools.Shared;

namespace Tools.Forms.Blumitech
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
            AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"},
                { "MyGourmetPOS","MGPOS2020"},
                { "Alta y Modificación de Artículos","ALTA2020"}

            };
            WebService = new WebService(DeviceId);
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
                brand = Info.Manufacturer;
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
                var Info = Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current;
                DeviceName = Info.DeviceName;
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
                var Info = Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current;
                Model = Info.Model;
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
                return Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.Platform.ToString();
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return brand;
        }

        public async Task<ProjectActivationState> IsAuthorizated()
        {
            //if (Plugin.Xamarin.Tools.Shared.Tools.Instance.Debugging
            //    || !Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.IsDevice
            //    )
            //{
            //    return true;
            //}
            //bool Autorized = false;
            ProjectActivationState state = ProjectActivationState.Unknown;
            //if (!DoIHaveInternet())
            //{
            //    state = ProjectActivationState.ConnectionFailed;
            //}
            //else
            //{
            state = await Autheticate(AppName);
            // }

            return state;
        }
        public async Task<bool> IsAuthorizated(Page page)
        {
            if (Plugin.Xamarin.Tools.Shared.Tools.Instance.Debugging
                || !Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.IsDevice
                )
            {
                return true;
            }
            bool Autorized = false;
            ProjectActivationState state = ProjectActivationState.Unknown;
            //if (!DoIHaveInternet())
            //{
            //    state = ProjectActivationState.ConnectionFailed;
            //}
            //else
            // {
            state = await Autheticate(AppName);
            //  }
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
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Este dispositivo debe ser registrado con una licencia valida antes de poder acceder a la aplicación", "Acceso denegado");
                    //     BlumLogin login = new BlumLogin(page.Background, this) as BlumLogin;
                    //   await page.Navigation.PushModalAsync(login, true);
                    Autorized = false;
                    break;
                case ProjectActivationState.ConnectionFailed:
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Revise su conexión a internet", "Atención");
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
#if NETCOREAPP


#else
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Revise su conexión a internet", "Atención");
#endif

                    return false;
                case "CUSTOMER_NOT_FOUND":
#if NETCOREAPP


#else
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Registro invalido", "Atención");
#endif
                    return false;
                case "PROJECT_NOT_FOUND":
#if NETCOREAPP


#else
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No esta contratado este servicio", "Atención");
#endif
                    return false;
            }
            if (!int.TryParse(response, out int left))
            {
                return false;
            }
            if (left <= 0)
            {
#if NETCOREAPP
#else
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No le quedan mas dispositivos para este proyecto", "Atención", "Ok");

#endif
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
#if NETCOREAPP
#else
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No le quedan mas dispositivos para este proyecto", "Atención", "Ok");
#endif
                        break;
                    case "PROJECT_NOT_ENROLLED":
#if NETCOREAPP
#else
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No esta contratado este servicio", "Atención");
#endif
                        break;
                    case "SUCCES":
                        left--;
#if NETCOREAPP
#else
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync($"Registro exitoso, le quedan: {left} dispositivos", "Atención");
#endif
                        return true;
                }
            }
            return false;
        }


        internal async Task<bool> Login(string UserName, string Password)
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
