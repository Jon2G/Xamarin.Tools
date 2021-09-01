using System.Diagnostics;
using System.Windows;
using Kit.Dialogs;
using Kit.License;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Lógica de interacción para DeviceRegister.xaml
    /// </summary>
    public partial class DeviceRegister : Window
    {
        public DeviceRegisterModel Model { get; private set; }

        public DeviceRegister(BlumAPI.License Licence, IDialogs Dialogs)
        {
            this.Model = new DeviceRegisterModel(Licence, Dialogs);
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
                this.Close();
            }
        }
    }
}