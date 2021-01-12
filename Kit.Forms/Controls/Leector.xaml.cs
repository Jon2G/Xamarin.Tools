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
            this.CodigoDeBarras = new CodigoDeBarras();
            InitializeComponent();
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
            this.CodigoEntrante?.Invoke(sender, e);
        }

        public async void Abrir()
        {
            if (!await Permisos.TenemosPermiso(Plugin.Permissions.Abstractions.Permission.Camera))
            {
                await Permisos.PedirPermiso(Plugin.Permissions.Abstractions.Permission.Camera);
            }
            this.CodigoDeBarras.ButtonCommand.Execute(null);
        }
    }
}