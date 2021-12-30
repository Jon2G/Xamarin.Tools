using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Kit.Dialogs;
using Kit.License;
using Kit.Services.Interfaces;
using Kit.WPF.Controls;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Lógica de interacción para DeviceRegister.xaml
    /// </summary>
    public partial class DeviceRegister : ICrossWindow
    {
        public DeviceRegisterModel Model { get; private set; }

        public DeviceRegister(BlumAPI.License Licence)
        {
            this.Model = new DeviceRegisterModel(Licence, this);
            this.DataContext = this.Model;
            InitializeComponent();
            this.TxtUsuario.DataContext = this.Model;
        }

        private void Hyperlink_RequestNavigate(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(BlumAPI.License.LoginSite));
            e.Handled = true;
        }

        private async void LogIn(object sender, RoutedEventArgs e)
        {
            this.Model.Password = Pssword.Password;
            if (await this.Model.LogIn())
            {
                Tools.Instance.SynchronizeInvoke.BeginInvokeOnMainThread(Close);
            }
        }
    }
}