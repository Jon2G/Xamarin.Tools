using System.Threading.Tasks;
using BaseLicense =BlumAPI.License;

namespace Kit.Forms.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(AppName)
        {
        }

        protected override async Task OpenRegisterForm()
        {
            var login =new Forms.Pages.DeviceRegister(this);
            await login.ShowDialog();
        }
    }
}