using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Plugin.Xamarin.Tools.Shared.Blumitech
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BlumLogin : ContentPage
    {
        public BlumLogin()
        {
            InitializeComponent();
        }
        private void MailChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.User.UserName) || string.IsNullOrEmpty(this.User.Password))
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

        }
    }
}