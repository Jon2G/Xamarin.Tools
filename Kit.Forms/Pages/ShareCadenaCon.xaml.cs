using Kit.Forms.Extensions;
using Kit.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kit.Forms.Services.Interfaces;
using Plugin.Media;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareCadenaCon : BasePopUp
    {
        public string Code { get; set; }
        public float Brightness { get; set; }
        public IBrightnessService BrightnessService { get; set; }
        public ShareCadenaCon(string Title, string Code)
        {
            this.BindingContext = this;
            this.Code = Code;
            InitializeComponent();
            this.TxtTitle.Text = Title;
            this.BrightnessService = DependencyService.Get<IBrightnessService>();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.Brightness = this.BrightnessService.GetBrightness();
            this.BrightnessService.SetBrightness(1);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.BrightnessService.SetBrightness(this.Brightness);
        }

        private async void SalvarQr(object sender, EventArgs e)
        {
            if (await Permisos.EnsurePermission<Permissions.StorageWrite>("Permita el acceso")==PermissionStatus.Granted)
            {
                ScreenshotResult screenshot = await Screenshot.CaptureAsync();
                using (Stream stream = await screenshot.OpenReadAsync())
                {
                    string app_name = Application.Current.GetType().Namespace;
                    await DependencyService.Get<IGalleryService>()
                         .SaveImageToGallery(stream, $"{TxtTitle.Text}_{Guid.NewGuid():N}.png", app_name);
                }

                Acr.UserDialogs.UserDialogs.Instance.Alert("La cadena ha sido salvada en su galeria");
            }


        }
    }
}