extern alias SharedZXingNet;

using SharedZXingNet::ZXing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit;
using Kit.Forms.Controls;
using Kit.Forms.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using System.Windows.Input;
using Command = Xamarin.Forms.Command;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Lector : ContentView
    {
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            propertyName: nameof(Color),
            returnType: typeof(Color), declaringType: typeof(Lector), defaultValue: Color.Black);

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set
            {
                SetValue(ColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty OnCodeReadCommandProperty =
            BindableProperty.Create(
              propertyName: nameof(OnCodeReadCommand),
              returnType: typeof(Xamarin.Forms.Command<string>),
              declaringType: typeof(Lector),
              defaultValue: new Xamarin.Forms.Command<string>((x) => Log.Logger.Debug("Código leido:{0}", x)),
              defaultBindingMode: BindingMode.OneWay);

        public Xamarin.Forms.Command<string> OnCodeReadCommand
        {
            get => (Xamarin.Forms.Command<string>)this.GetValue(Lector.OnCodeReadCommandProperty);
            set
            {
                SetValue(Lector.OnCodeReadCommandProperty, value);
            }
        }

        private string _Barcode;

        public string Barcode
        {
            get => _Barcode;
            set
            {
                _Barcode = value;
                OnPropertyChanged();
            }
        }

        private ICommand _OpenCameraCommand;

        public ICommand OpenCameraCommand
        {
            get => _OpenCameraCommand;
            private set
            {
                _OpenCameraCommand = value;
                OnPropertyChanged();
            }
        }

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

        public Lector() : this(AllFormats)
        {
        }

        public Lector(params BarcodeFormat[] BarcodeFormats)
        {
            InitializeComponent();
            Init(BarcodeFormats);
        }

        public void Init(params BarcodeFormat[] BarcodeFormats)
        {
            this.OpenCameraCommand = new Kit.Extensions.Command(OpenCamera);
            this.BarcodeFormats = new List<BarcodeFormat>(BarcodeFormats);
            if (this.BarcodeFormats.Count <= 0)
            {
                this.BarcodeFormats.Add(BarcodeFormat.QR_CODE);
                this.BarcodeFormats.Add(BarcodeFormat.CODE_128);
                this.BarcodeFormats.Add(BarcodeFormat.EAN_13);
            }
        }

        private void OpenCamera()
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
                        Barcode = null;
                    }
                    else
                    {
                        Barcode = result.Text;
                    }
                    OnCodeReadCommand?.Execute(Barcode);
                });
            };
            Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(page) { BarTextColor = Color.White, BarBackgroundColor = Color.CadetBlue }, true);
        }

        public async void Abrir()
        {
            if (this.BarcodeFormats is null || !this.BarcodeFormats.Any())
            {
                throw new WarningException("Please call Init before attemping to open this Reader");
            }
            await Permisos.EnsurePermission<Permissions.Camera>("Por favor permita el acceso");
            this.OpenCameraCommand.Execute(null);
        }
    }
}