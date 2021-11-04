extern alias SharedZXingNet;

using SharedZXingNet::ZXing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kit.Forms.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Windows.Input;
using Command = Xamarin.Forms.Command;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;
using AsyncAwaitBestPractices.MVVM;
using AsyncAwaitBestPractices;

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

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            propertyName: nameof(FontSize),
            returnType: typeof(double), declaringType: typeof(Lector), defaultValue: 14d);

        [Xamarin.Forms.TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set
            {
                SetValue(FontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty OnCodeReadCommandProperty =
            BindableProperty.Create(
              propertyName: nameof(OnCodeReadCommand),
              returnType: typeof(AsyncCommand<string>),
              declaringType: typeof(Lector),
              //defaultValue: new Xamarin.Forms.Command<string>(
              //    (x) => Log.Logger.Debug("Código leido:{0}", x)),
              defaultBindingMode: BindingMode.TwoWay,
              propertyChanged: OnCodeReadCommandPropertyChanged);

        private static void OnCodeReadCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Lector lector)
            {
                lector.OnCodeReadCommand = newValue as AsyncCommand<string>;
            }
        }

        public AsyncCommand<string> OnCodeReadCommand
        {
            get => (AsyncCommand<string>)this.GetValue(Lector.OnCodeReadCommandProperty);
            set
            {
                SetValue(Lector.OnCodeReadCommandProperty, value);
                OnPropertyChanged();
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

        private ZXingScannerPage Page;
        private INavigation INavigation;
        private bool IsShell;
        private ICommand _CloseCommand;
        private ICommand CloseCommand => _CloseCommand ??= new Command(Close);

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

        private void Close()
        {
            if (IsShell)
            {
                INavigation.PopAsync();
            }
            else
            {
                INavigation.PopModalAsync();
            }
        }

        private void OnDisappearing(object sender, EventArgs e)
        {
            Page.OnScanResult -= OnScanResult;
            Page.Disappearing -= OnDisappearing;
            this.Page.ToolbarItems?.Clear();
            this.Page = null;
        }

        private ZXingScannerPage BuildPage()
        {
            this.Page = new ZXingScannerPage(new MobileBarcodeScanningOptions()
            {
                PossibleFormats = this.BarcodeFormats
            })
            {
                Title = "Leector de codigos de barras",
            };
            this.Page.ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Cerrar",
                Command = CloseCommand
            });
            Page.OnScanResult += OnScanResult;
            Page.Disappearing += OnDisappearing;
            return this.Page;
        }

        private void OpenCamera()
        {
            if (this.Page is not null)
            {
                return;
            }
            this.IsShell = (Shell.Current is not null);
            this.INavigation = IsShell ? Shell.Current.Navigation : Application.Current.MainPage.Navigation;
            BuildPage();
            Device.BeginInvokeOnMainThread(() =>
            {
                if (this.IsShell)
                {
                    this.INavigation.PushAsync(Page);
                }
                else
                {
                    this.INavigation.PushModalAsync(new NavigationPage(Page) { BarTextColor = Color.White, BarBackgroundColor = Color.CadetBlue }, true);
                }
            });
        }

        private void OnScanResult(Result result)
        {
            this.Page.IsScanning = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                CloseCommand.Execute(this);
                if (string.IsNullOrEmpty(result.Text))
                {
                    Barcode = null;
                }
                else
                {
                    Barcode = result.Text;
                }
                OnCodeReadCommand?.ExecuteAsync(Barcode).SafeFireAndForget();
            });
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