using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Extensions;
using Kit.Forms.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Leector : ContentView
    {
        public event EventHandler CodigoEntrante;
        public string CodigoBarras => this.CodigoDeBarras.Result;
        private CodigoDeBarras CodigoDeBarras { get; set; }
        public Leector()
        {
            InitializeComponent();
        }
        public Leector(params BarcodeFormat[] BarcodeFormats)
        {
            InitializeComponent();
            Init(BarcodeFormats);
        }
        public void Init(params BarcodeFormat[] BarcodeFormats)
        {
            this.CodigoDeBarras = new CodigoDeBarras(BarcodeFormats);
            this.ImgCamera.BindingContext = this.CodigoDeBarras;
            this.ImgCamera.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                NumberOfTapsRequired = 1,
                Command = this.CodigoDeBarras.ButtonCommand
            });
            this.CodigoDeBarras.PropertyChanged += CodigoCambio;
        }

        private void CodigoCambio(object sender, PropertyChangedEventArgs e)
        {
            this.CodigoEntrante?.Invoke(this, e);
        }

        public async void Abrir()
        {
            if(this.CodigoDeBarras is null)
            {
                throw new WarningException("Please call Init before attemping to open this Reader");
            }
            if (!await Permisos.TenemosPermiso(Plugin.Permissions.Abstractions.Permission.Camera))
            {
                await Permisos.PedirPermiso(Plugin.Permissions.Abstractions.Permission.Camera);
            }
            this.CodigoDeBarras.ButtonCommand.Execute(null);
        }

    }
}