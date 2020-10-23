
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.License;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tools.Forms.Blumitech
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BlumLogin : Page
    {
        private string _UserName;
        private string _Password;

        public Command SubmitCommand { get; set; }
        public bool IsValidated { get; set; }
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }
        public Licence Licence { get; set; }

        public BlumLogin(Brush brush, Licence Licence)
        {
            SubmitCommand = new Command(LogIn);
            this.Background = brush;
            InitializeComponent();
            this.Licence = Licence;
        }
        private void MailChanged(object sender, EventArgs e)
        {
            if(Btn is null)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
            {
                this.Btn.TextColor = Color.Gray;
            }
            else
            {
                this.Btn.TextColor = Color.White;
            }
        }
        private void PasswordChanged(object sender, EventArgs e)
        {
            MailChanged(sender, e);
        }
        private void Registrarse(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://ecommerce.blumitech.com.mx/"));
        }
        private async void LogIn()
        {
            UserName = TxtUser.Text;
            Password = TxtPass.Text;
            IsValidated = !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            if (!IsValidated)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe llenar todos los campos correctamente", "Alert", "OK");
                return;
            }
            if (await this.Licence.Login(UserName, Password))
            {
                if (await Licence.RegisterDevice(UserName, Password))
                {
                    await this.Navigation.PopModalAsync();
                }
            }
            else
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Usuario o contraseña incorrectos", "Alert", "OK");
            }
        }
        private void Btn_Clicked(object sender, EventArgs e)
        {
            this.SubmitCommand.Execute(this);
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}