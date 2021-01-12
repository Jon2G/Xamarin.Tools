using Kit.Forms.Controls.Pages;
using Kit.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ShareCadenaCon(string Code, IBrightnessService BrightnessService)
        {
            this.Code = Code;
            this.BrightnessService = BrightnessService;
            InitializeComponent();
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
    }
}