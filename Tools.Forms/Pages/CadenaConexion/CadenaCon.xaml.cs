using Acr.UserDialogs;
using Tools.Forms.Controls.Pages.CadenaConexion;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Log = SQLHelper.Log;
using Plugin.Xamarin.Tools.Shared.Converters;

namespace Tools.Forms.Controls.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadenaCon : BasePage
    {
        public static readonly BindableProperty LogoProperty = BindableProperty.Create(
            propertyName: nameof(Logo), returnType: typeof(ImageSource), declaringType: typeof(CadenaCon), defaultValue: null);
        [TypeConverter(typeof(MyImageSourceConverter))]
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

        public event EventHandler Confirmado;
        private Configuracion Configuracion;
        private readonly SQLHLite DBConection;
        public SQLH NewDBConection { get; private set; }
        public CadenaCon(SQLHLite DBConection)
        {
            InitializeComponent();
            this.DBConection = DBConection;
            this.Configuracion = Configuracion.ObtenerConfiguracion(this.DBConection);
            this.NewDBConection = new SQLH(this.Configuracion.CadenaCon);
        }
        public static bool IsActive()
        {
            if ((Application.Current.MainPage.Navigation.ModalStack.Any() && Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() is CadenaCon)
                || (Application.Current.MainPage.Navigation.NavigationStack.Any() && Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is CadenaCon))
            {
                return true;
            }
            return false;
        }
        public CadenaCon(SQLHLite DBConection,Exception ex)
        {
            InitializeComponent();
            this.DBConection = DBConection;
            this.Configuracion = Configuracion.ObtenerConfiguracion(this.DBConection);
            this.NewDBConection = new SQLH(this.Configuracion.CadenaCon);
            ToogleStatus(ex);
        }
        public CadenaCon(SQLHLite DBConection,Configuracion Configuracion)
        {
            InitializeComponent();
            try
            {
                this.DBConection = DBConection;
                this.Configuracion = Configuracion;
                this.NewDBConection = new SQLH(this.Configuracion.CadenaCon);
                this.TxtCadenaCon.Text = this.Configuracion.CadenaCon;
            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al leer la cadena de conexion para editar desde cadenacon.cs");
            }
        }
        private void BasePage_Appearing(object sender, EventArgs e)
        {
            this.LockOrientation(DeviceOrientation.Portrait);
            TxtDbName.Text = this.Configuracion.NombreDB;
            TxtContraseña.Text = this.Configuracion.Password;
            TxtPuerto.Text = this.Configuracion.Puerto;
            TxtServidor.Text = this.Configuracion.Servidor;
            TxtUsuario.Text = this.Configuracion.Usuario;
            TxtCadenaCon.Text = this.Configuracion.CadenaCon;
        }
        private void BasePage_Disappearing(object sender, EventArgs e)
        {
            //Confirmado?.Invoke(sender, e);
            //Confirmado = null;
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
                ex = await this.NewDBConection.PruebaConexion(Test);
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

                    if (this.NewDBConection.ExisteTabla("DESCARGAS_VERSIONES"))
                    {
                        this.NewDBConection.EXEC(
                            "DELETE FROM DESCARGAS_VERSIONES WHERE ID_DISPOSITIVO = @ID_DISPOSITIVO",
                            System.Data.CommandType.Text, false,
                            new SqlParameter("ID_DISPOSITIVO", Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.DeviceId));
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
                SQLHelper.Log.LogMe(exx, "Al intentar cambiar la cadena de conexión desde CadenaCon.cs GuardarAsync");
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
            Exception ex = await this.NewDBConection.PruebaConexion(Test);
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
                    Log.LogMe("Se elimino la base de datos");
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Se elimino la base de datos local", "Atención", "Ok");
                    Guardar(sender, e);
                }
            }
        }
    }
}