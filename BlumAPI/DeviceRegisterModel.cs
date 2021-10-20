using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Kit.Dialogs;
using Kit.Model;
using Kit.Services.Interfaces;

namespace Kit.License
{
    public sealed class DeviceRegisterModel : ModelBase
    {
        private string _UserName;
        private string _Password;
        public bool IsValidated { get; set; }

        public string UserName
        {
            get => _UserName; set
            {
                _UserName = value; Raise(() => UserName);
            }
        }

        public string Password
        {
            get => _Password; set
            {
                _Password = value; Raise(() => Password);
            }
        }

        public BlumAPI.License Licence { get; set; }
        private readonly IDialogs Dialogs;

        private ICommand _LogInCommand;
        public ICommand LogInCommand => _LogInCommand ??= new AsyncCommand(LogIn);
        private readonly ICrossWindow Window;

        public DeviceRegisterModel(BlumAPI.License licence, IDialogs Dialogs, ICrossWindow Window)
        {
            this.Window = Window;
            this.Dialogs = Dialogs;
            this.Licence = licence;
        }

        public async Task<bool> LogIn()
        {
            await Task.Yield();
            IsValidated = !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            if (!IsValidated)
            {
                await Dialogs.CustomMessageBox.ShowOK("Debe llenar todos los campos correctamente", "Alerta", "OK");
                return false;
            }
            if (await this.Licence.Login(UserName, Password))
            {
                if (await Licence.RegisterDevice(UserName, Password))
                {
                    Tools.Instance.SynchronizeInvoke.BeginInvokeOnMainThread(() => this.Window.Close().SafeFireAndForget());
                    return true;
                }
            }
            else
            {
                await Dialogs.CustomMessageBox.ShowOK("Usuario o contraseña incorrectos", "Alerta", "OK");
            }
            return false;
        }
    }
}