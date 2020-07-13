using Plugin.Xamarin.Tools.Shared.Services;
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
        private Licence(string DeviceId)
        {
            this.AppKeys = new Dictionary<string, string>() {
                { "InventarioFisico","INVIS001"}
            };
            this.WebService = new WebService(DeviceId);
        }
        public static Licence Instance(AbstractTools instance)
        {
            IDeviceInfo deviceInfo = DependencyService.Get<IDeviceInfo>();
            return new Licence(deviceInfo.Id);
        }
        public async Task<ProjectActivationState> Autheticate(string AppKey)
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
