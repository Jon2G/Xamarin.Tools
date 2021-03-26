using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Kit.CadenaConexion;
using Kit.Forms.Controls;
using Kit.Daemon;
using Kit.Forms.Services;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using Kit.Sql.Helpers;
using Kit.Sql.SqlServer;
using Xamarin.Essentials;
using SQLiteConnection = Kit.Sql.Sqlite.SQLiteConnection;
using ZXing;



namespace Kit.Forms.Pages
{
    extern alias SharedZXingNet;


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
                Raise(() => LogoProperty);
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
                Raise(() => IsLogoVisible);
            }
        }
        public Lector Leector { get; set; }
        public event EventHandler Confirmado;
        private Configuracion Configuracion;
        private readonly Kit.Sql.Sqlite.SQLiteConnection DBConection;
        private readonly IDeviceInfo DeviceInfo;
        public SQLServerConnection NewDBConection { get; private set; }
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
        public CadenaCon(IDeviceInfo DeviceInfo, SQLiteConnection DBConection)
        {
            this.DeviceInfo = DeviceInfo;
            InitializeComponent();
            this.DBConection = DBConection;
            this.Configuracion = Configuracion.ObtenerConfiguracion(this.DBConection, DeviceInfo.DeviceId);
            this.NewDBConection = new SQLServerConnection(this.Configuracion.CadenaCon);
        }
        public CadenaCon(IDeviceInfo DeviceInfo, SQLiteConnection DBConection, Exception ex)
        {
            this.DeviceInfo = DeviceInfo;
            InitializeComponent();
            this.DBConection = DBConection;
            this.Configuracion = Configuracion.ObtenerConfiguracion(this.DBConection, DeviceInfo.DeviceId);
            this.NewDBConection = new SQLServerConnection(this.Configuracion.CadenaCon);
            ToogleStatus(ex);
        }
        public CadenaCon(IDeviceInfo DeviceInfo, SQLiteConnection DBConection, Configuracion Configuracion)
        {
            this.DeviceInfo = DeviceInfo;
            InitializeComponent();
            try
            {
                this.DBConection = DBConection;
                this.Configuracion = Configuracion;
                this.NewDBConection = new SQLServerConnection(this.Configuracion.CadenaCon);
                this.TxtCadenaCon.Text = this.Configuracion.CadenaCon;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al leer la cadena de conexion para editar desde cadenacon.cs");
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
                ex = this.NewDBConection.TestConnection(Test);
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
                    this.Configuracion.Empresa = this.Configuracion.NombreDB;
                    this.Configuracion.Salvar(this.DBConection, this.NewDBConection);

                    this.NewDBConection.ConnectionString = new SqlConnectionStringBuilder(this.Configuracion.CadenaCon);

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
                Log.Logger.Error(exx, "Al intentar cambiar la cadena de conexión desde CadenaCon.cs GuardarAsync");
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

            Exception ex = this.NewDBConection.TestConnection(Test);
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
            LblIcon.Text = ex is null ? "Ok." : "X";
        }
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(TxtEstado.Text, "Información", "Ok");
        }
        private async void GuardarRenovar_Touched(object sender, EventArgs e)
        {
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Espere un momento..."))
            {
                if (EliminarDB(DBConection.DatabasePath))
                {
                    Log.Logger.Warning("Se elimino la base de datos");
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Se elimino la base de datos local", "Atención", "Ok");
                    Guardar(sender, e);
                }
            }
        }
        public bool EliminarDB(string RutaDb)
        {
            try
            {
                FileInfo file = new FileInfo(RutaDb);
                if (file.Exists)
                {
                    File.Delete(RutaDb);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al eliminar la base de datos de sqlite");
                return false;
            }
        }
        private async void Importar_Touched(object sender, EventArgs e)
        {
            ActionSheetConfig config = new ActionSheetConfig()
            {
                Title = "Importar cadena de conexión",
                Cancel = new ActionSheetOption("Cancelar"),
                Options = new List<ActionSheetOption>()
                {
                    new ActionSheetOption( "Usar camara",FromCamera),
                    new ActionSheetOption("Seleccionar desde la galería",FromGallery)
                },
                UseBottomSheet = true
            };
            UserDialogs.Instance.ActionSheet(config);
        }

        private async void FromGallery()
        {
            FileResult qr = await MediaPicker.PickPhotoAsync(new MediaPickerOptions()
            {
                Title = "Importar cadena de conexión"
            });
            if (qr is null)
            {
                return;
            }
            BarcodeDecoding reader = new BarcodeDecoding();
            SharedZXingNet::ZXing.Result
             result = await reader.Decode(new FileInfo(qr.FullPath), SharedZXingNet::ZXing.BarcodeFormat.QR_CODE
                , new[]
                {
                    new KeyValuePair<SharedZXingNet::ZXing.DecodeHintType, object>(SharedZXingNet::ZXing.DecodeHintType.TRY_HARDER,null)
                });
            Deserialize(result?.Text);
        }
        private void FromCamera()
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
                Deserialize(leector.CodigoBarras);
            }
        }

        private void Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                Configuracion configuracion_qr = Configuracion.DeSerialize(json);
                if (configuracion_qr != null)
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