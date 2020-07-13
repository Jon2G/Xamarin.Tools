using Plugin.Xamarin.Tools.Shared.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Blumitech
{
    public class LoginViewModel : ViewModelBase
    {
        private string _UserName;
        private string _Password;

        public LoginViewModel()
        {
            SubmitCommand = new Command(LogIn);
        }
        public Command SubmitCommand { get; set; }
        public bool IsValidated { get; set; }
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }

        async void LogIn()
        {
            if (!IsValidated)
            {
                await Application.Current.MainPage.DisplayAlert("", "Debe llenar todos los campos correctamente", "OK");
            }


        }

    }

}
