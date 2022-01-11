using Microsoft.Maui;using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Kit.MAUI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OtgCodeBarScannerDetectorPage : ContentPage
    {
        public OtgCodeBarScannerDetectorPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.Model.Init(BeginView,CenterView,EndView);
        }
    }
}