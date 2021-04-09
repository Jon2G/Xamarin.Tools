extern alias SharedZXingNet;
using SharedZXingNet::ZXing;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public static BarcodeFormat[] AllFormats 
        {
            get
            {
                return new[]
                {
                    BarcodeFormat.AZTEC, BarcodeFormat.CODABAR, BarcodeFormat.CODE_39,
                    BarcodeFormat.CODE_93, BarcodeFormat.CODE_128, BarcodeFormat.DATA_MATRIX,
                    BarcodeFormat.EAN_8, BarcodeFormat.EAN_13, BarcodeFormat.ITF,
                    BarcodeFormat.MAXICODE, BarcodeFormat.PDF_417, BarcodeFormat.QR_CODE,
                    BarcodeFormat.RSS_14, BarcodeFormat.RSS_EXPANDED, BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E, BarcodeFormat.All_1D, BarcodeFormat.UPC_EAN_EXTENSION,
                    BarcodeFormat.MSI, BarcodeFormat.PLESSEY, BarcodeFormat.IMB
                };
            }
        }
    

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
            MobileBarcodeScanningOptions options = new MobileBarcodeScanningOptions();
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
