using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Kit.CadenaConexion;
using Kit.Forms.Controls;
using ZXing;
using Kit.Daemon;
using Kit.Sql.Helpers;

namespace Kit.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadenaCon : BasePage
    {
        public static readonly BindableProperty LogoProperty = BindableProperty.Create(
            propertyName: nameof(Logo), returnType: typeof(ImageSource), declaringType: typeof(CadenaCon), defaultValue: null);
        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource Logo
        {
            get { return (ImageSource)GetValue(LogoProperty); }
            set
            {
                SetValue(LogoProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IsLogoVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsLogoVisible), returnType: typeof(bool), declaringType: typeof(CadenaCon), defaultValue: false);
        public bool IsLogoVisible
        {
            get { return (bool)GetValue(IsLogoVisibleProperty); }
            set
            {
                SetValue(IsLogoVisibleProperty, value);
                OnPropertyChanged();
            }
        }
        public Lector Leector { get; set; }
        public event EventHandler Confirmado;
        private Configuracion Configuracion;
        private readonly SqLite DBConection;
        private readonly Services.Interfaces.IDeviceInfo DeviceInfo;
        public SqlServer NewDBConection { get; private set; }
        public bool ShouldSetUpDaemon { get; set; }
        public static bool IsActive()
        {
            if ((Application.Current.MainPage.Navigation.ModalStack.Any() && Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() is CadenaCon)
                || (Application.Current.MainPage.Navigation.NavigationStack.Any() && Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is CadenaCon))
            {
                return true;
            }
            return false;
        }
        public CadenaCon(Services.Interfaces.IDeviceInfo DeviceInfo, SqLite DBConection)
        {
            this.DeviceInfo = DeviceInfo;
            InitializeComponent();
            this.DBConection = DBConection;
            this.Configuracion = Configuracion.ObtenerConfiguracion(this.DBConection, DeviceInfo.DeviceId);
            this.NewDBConection = new SqlServer(this.Configuracion.CadenaCon);
        }
        public CadenaCon(Services.Interfaces.IDeviceInfo DeviceInfo, SqLite DBConection, Exception ex)
        {
            this.DeviceInfo = DeviceInfo;
            InitializeComponent();
            this.DBConection = DBConection;
            this.Configuracion = Configuracion.ObtenerConfiguracion(this.DBConection, DeviceInfo.DeviceId);
            this.NewDBConection = new SqlServer(this.Configuracion.CadenaCon);
            ToogleStatus(ex);
        }
        public CadenaCon(Services.Interfaces.IDeviceInfo DeviceInfo, SqLite DBConection, Configuracion Configuracion)
        {
            this.DeviceInfo = DeviceInfo;
            InitializeComponent();
            try
            {
                this.DBConection = DBConection;
                this.Configuracion = Configuracion;
                this.NewDBConection = new SqlServer(this.Configuracion.CadenaCon);
                this.TxtCadenaCon.Text = this.Configuracion.CadenaCon;
            }
            catch (Exception ex)
            {
                Kit.Sql.Log.LogMe(ex, "Al leer la cadena de conexion para editar desde cadenacon.cs");
            }
        }
        private void BasePage_Appearing(object sender, EventArgs e)
        {
            this.LockOrientation(DeviceOrientation.Portrait);
            CargarCadena();
        }
        private void CargarCadena()
        {
            TxtDbName.Text = this.Configuracion.NombreDB;
            TxtContraseña.Text = this.Configuracion.Password;
            TxtPuerto.Text = this.Configuracion.Puerto;
            TxtServidor.Text = this.Configuracion.Servidor;
            TxtUsuario.Text = this.Configuracion.Usuario;
            TxtCadenaCon.Text = this.Configuracion.CadenaCon;
        }
        private void BasePage_Disappearing(object sender, EventArgs e)
        {
            if (this.Leector != null)
            {
                this.Leector.CodigoEntrante -= Leector_CodigoEntrante;
            }
        }
        private void RecargaCadena(object sender, TextChangedEventArgs e)
        {
            string usuario = TxtUsuario.Text?.Trim()?.Replace(" ", "") ?? string.Empty;
            string password = TxtContraseña.Text?.Trim()?.Replace(" ", "") ?? string.Empty;
            string puerto = TxtPuerto.Text?.Trim()?.Replace(" ", "") ?? string.Empty;
            string servidor = TxtServidor.Text?.Trim()?.Replace(" ", "") ?? string.Empty;
            string dbName = TxtDbName.Text?.Trim()?.Replace(" ", "") ?? string.Empty;
            TxtCadenaCon.Text = Configuracion.BuildFrom(dbName, password, puerto, servidor, usuario).CadenaCon;
        }
        private async void Guardar(object sender, EventArgs e)
        {
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Espere un momento..."))
            {
                await GuardarAsync();
            }
        }
        private async Task GuardarAsync()
        {
            try
            {
                string Test = TxtCadenaCon.Text;
                Exception ex;
                //using (Acr.UserDialogs.UserDialogs.Instance.Loading("Intentando conectar..."))
                //{
                ex = this.NewDBConection.PruebaConexion(Test);
                //}
                ToogleStatus(ex);
                if (ex != null)
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync($"La conexión actual no es valida.\n{ex.Message}", "Mensaje informativo");

                }
                else
                {
                    this.Configuracion = Configuracion.BuildFrom(TxtDbName.Text, TxtContraseña.Text, TxtPuerto.Text, TxtServidor.Text, TxtUsuario.Text);
                    this.Configuracion.CadenaCon = TxtCadenaCon.Text;
                    this.Configuracion.Salvar(this.DBConection, this.NewDBConection);

                    this.NewDBConection.ChangeConnectionString(this.Configuracion.CadenaCon);

                    if (this.ShouldSetUpDaemon)
                    {
                        Daemon.Daemon.Current.SetUp(NewDBConection);
                    }

                    if (this.Navigation.ModalStack.Count > 0)
                    {
                        await Navigation.PopModalAsync();
                    }
                    Confirmado?.Invoke(this, EventArgs.Empty);
                    Confirmado = null;
                }
            }
            catch (Exception exx)
            {
                Kit.Sql.Log.LogMe(exx, "Al intentar cambiar la cadena de conexión desde CadenaCon.cs GuardarAsync");
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(exx.Message, "Alerta");
            }
        }
        private void Cancelar(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        protected override bool OnBackButtonPressed()
        {
            if (this.IsModalLocked)
            {
                return true;
            }
            Navigation.PopModalAsync(true);
            Confirmado?.Invoke(this, EventArgs.Empty);
            Confirmado = null;
            return true;

        }
        private async void ProbarConexion(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Intentando conectar...", MaskType.Black);
            await ProbarConexionAsync(TxtCadenaCon.Text.Replace(Environment.NewLine, "").Trim());
            UserDialogs.Instance.HideLoading();
        }
        private async Task<bool> ProbarConexionAsync(string Test, bool MostrarMensajesStatus = true)
        {
            await Task.Yield();
            Exception ex = this.NewDBConection.PruebaConexion(Test);
            if (ex != null)
            {
                if (MostrarMensajesStatus)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync($"La cadena de conexión no es valida.\n{ex.Message}", "Mensaje informativo");
                    });
                }
                return false;
            }
            ToogleStatus(ex);
            if (MostrarMensajesStatus)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Conexión correcta", "Mensaje informativo");
                });
            }
            return true;
        }
        private void ToogleStatus(Exception ex)
        {
            TxtEstado.Text = ex?.Message ?? "Correcto";
            Img.Source = ex is null ? "usbok.png" : "usbbad.png";
        }
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(TxtEstado.Text, "Información", "Ok");
        }
        private async void GuardarRenovar_Touched(object sender, EventArgs e)
        {
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Espere un momento..."))
            {
                if (DBConection.EliminarDB())
                {
                    DBConection.RevisarBaseDatos();
                    Kit.Sql.Log.LogMe("Se elimino la base de datos");
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Se elimino la base de datos local", "Atención", "Ok");
                    Guardar(sender, e);
                }
            }
        }
        private void Importar_Touched(object sender, EventArgs e)
        {
            if (this.Leector is null)
            {
                this.Leector = new Lector(BarcodeFormat.QR_CODE);
                this.Leector.CodigoEntrante += Leector_CodigoEntrante;
            }
            this.Leector.Abrir();
        }
        private void Leector_CodigoEntrante(object sender, EventArgs e)
        {
            if (sender is Lector leector)
            {
                leector.CodigoEntrante -= Leector_CodigoEntrante;
                if (!string.IsNullOrEmpty(leector.CodigoBarras))
                {
                    Configuracion configuracion_qr = Configuracion.DeSerialize(leector.CodigoBarras);
                    if(configuracion_qr != null)
                    {
                        this.Configuracion = configuracion_qr;
                        this.CargarCadena();
                    }
                    else
                    {
                        Acr.UserDialogs.UserDialogs.Instance.Alert("Formato Qr incorrecto", "Incorrecto", "Ok");
                    }
                }
            }
        }
    }
}