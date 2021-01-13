using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace Kit.Forms.Controls
{
    public class CodigoDeBarras : INotifyPropertyChanged
    {
        private string _result;
        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        public ICommand ButtonCommand { get; private set; }
        public List<BarcodeFormat> BarcodeFormats { get; set; }
        public CodigoDeBarras(params BarcodeFormat[] BarcodeFormats)
        {
            ButtonCommand = new Command(OnButtomCommand);
            this.BarcodeFormats = new List<BarcodeFormat>(BarcodeFormats);
            if (this.BarcodeFormats.Count <= 0)
            {
                this.BarcodeFormats.Add(BarcodeFormat.QR_CODE);
                this.BarcodeFormats.Add(BarcodeFormat.CODE_128);
                this.BarcodeFormats.Add(BarcodeFormat.EAN_13);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private void OnButtomCommand()
        {
            var options = new MobileBarcodeScanningOptions();
            options.PossibleFormats = this.BarcodeFormats;
            ZXingScannerPage page = new ZXingScannerPage(options) { Title = "Leector de codigos de barras" };
            ToolbarItem closeItem = new ToolbarItem { Text = "Cerrar" };
            closeItem.Clicked += (sender, e) =>
            {
                page.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage.Navigation.PopModalAsync();
                });
            };
            page.ToolbarItems.Add(closeItem);
            page.OnScanResult += (result) =>
            {
                page.IsScanning = false;

                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage.Navigation.PopModalAsync();
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        Result = "Codigo de barras invalido";
                    }
                    else
                    {
                        Result = result.Text;
                    }
                });
            };
            Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(page) { BarTextColor = Color.White, BarBackgroundColor = Color.CadetBlue }, true);
        }
    }
}
