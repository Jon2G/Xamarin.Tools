
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
        private Func<object, EventArgs, Licence> OnLoginSuccesEvent;
        private readonly Dictionary<string, string> AppKeys;
        private readonly WebService WebService;
        private readonly string AppName;
        private Licence(string DeviceId,string AppName)
        {
            this.AppName = AppName;
            this.AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"}
            };
            this.WebService = new WebService(DeviceId);
        }
        public static Licence Instance(AbstractTools instance, string AppName)
        {
            IDeviceInfo deviceInfo = DependencyService.Get<IDeviceInfo>();
            return new Licence(deviceInfo.DeviceId, AppName);
        }
        public async Task<bool> IsAuthorizated(Page page)
        {
            bool Autorized = false;
            ProjectActivationState state
                = await Autheticate(this.AppName);
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
                    BlumLogin login = new BlumLogin().LockModal() as BlumLogin;
                    login.GetUser().LoginSucces=new Command(Login_Disappearing);
                    await page.Navigation.PushModalAsync(login);
                    Autorized = false;
                    break;
                case ProjectActivationState.ConnectionFailed:
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Revise su conexión a internet", "Atención");
                    break;
            }
            return Autorized;
        }

        private void Login_Disappearing(object sender)
        {
            BlumLogin login = (sender as BlumLogin);
            login.GetUser().LoginSucces = null;
            this.OnLoginSuccesEvent?.Invoke(this, EventArgs.Empty);
        }

        public Licence OnLoginSucces(Func<object, EventArgs, Licence> loginSucces)
        {
            this.OnLoginSuccesEvent = loginSucces;
            return this;
        }

        private async Task<ProjectActivationState> Autheticate(string AppKey)
        {
            if (!this.AppKeys.ContainsKey(AppKey))
            {
                return ProjectActivationState.Denied;
            }
            AppKey = this.AppKeys[AppKey];
            return await this.WebService.RequestProjectAccess(AppKey);
        }


    }
}
