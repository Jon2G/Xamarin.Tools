using Plugin.Xamarin.Tools.Shared.Enums;
using Plugin.Xamarin.Tools.Shared.Models.CadenaCon;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tools.Forms.Controls.Pages.CadenaConexion;

namespace Tools.WPF.Forms.Windows
{
    /// <summary>
    /// Lógica de interacción para CadenaCon.xaml
    /// </summary>
    public partial class CadenaCon : Window
    {
        public readonly SQLH Connection;
        public readonly SQLHLite LiteConnection;
        public CadenaCon(SQLHLite LiteConnection, SQLH Connection, Exception ex)
        {
            this.LiteConnection = LiteConnection;
            this.Connection = Connection;
            InitializeComponent();
            TxtStatus.Text = ex.Message;
            ImgStatus.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\002-usb-1.png");
        }
        public CadenaCon(SQLHLite LiteConnection, SQLH Connection)
        {
            this.LiteConnection = LiteConnection;
            this.Connection = Connection;
            InitializeComponent();
            TxtStatus.Text = "La cadena de conexion es correcta";
            ImgStatus.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\001-usb.png");
        }

        private void CadenaCon_OnLoaded(object sender, RoutedEventArgs e)
        {
            Try.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\003-wire.png");
            Save.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\save.png");
            Error.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\error.png");

            TxtCadenaCon.Text = this.Connection.ConnectionString;
            CmbxEmpresas.ItemsSource = Empresas.ListarEmpresas(this.LiteConnection);
        }

        private async void Guardar(object sender, RoutedEventArgs e)
        {

            try
            {
                //await Demon.Current.Sleep();
                if (this.Connection.PruebaConexion(TxtCadenaCon.Text) is Exception ex)
                {
                    if (Plugin.Xamarin.Tools.Shared.Services.CustomMessageBox.Current.ShowYesNo(
                            "La conexión actual no es valida\n" + ex.Message + "\n¿Desea guardarla de todas formas?",
                            "La conexión no es valida", "Si", "No", CustomMessageBoxImage.Question) == CustomMessageBoxResult.No)
                    {
                        return;
                    }
                }
                this.Connection.ChangeConnectionString(TxtCadenaCon.Text);
                Configuracion configuracion = new Configuracion(this.Connection.ConnectionString)
                {
                    NombreDB = TxtDbName.Text,
                    Usuario = TxtUsuario.Text,
                    Password = TxtContraseña.Text,
                    Empresa = CmbxEmpresas.Text,
                    Puerto = TxtPuerto.Text,
                    Servidor = TxtServidor.Text
                };
                configuracion.Salvar(this.LiteConnection, this.Connection);
                EstableceCadenaCon(configuracion);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
        }
        public void EstableceCadenaCon(Configuracion configuracion)
        {
            //Eliminar referencias anteriores
            this.LiteConnection.EXEC($"DELETE FROM configuracion WHERE NOMBRE='{configuracion.Empresa}' OR NOMBRE ='EMPRESA, S.A. DE C.V.'");
            //DESABILITAR AUTO CADENA
            this.LiteConnection.EXEC("UPDATE configuracion SET CLAVE=''");
            configuracion.CadenaCon = "Provider=SQLNCLI;" +
                           configuracion.CadenaCon.
                               Replace("Integrated Security=False;", "").
                               Replace("Persist Security Info=True;", "").Trim();

            this.LiteConnection.EXEC(
                "INSERT INTO CONFIGURACION " +
                "(NOMBRE,cadenadeconexion,security,usuarios,datos,Direccion1,Direccion2,CP,Telefonos,Email,Web,ODBC,SERVIDOR,PUERTO,MyUser,Password,Clave) VALUES" +
                $@"(""{configuracion.Empresa}"",""{configuracion.CadenaCon}"",""{"Windows"}"",""{"0"}"",""{configuracion.NombreDB}"",""{""}"",""{""}"",""{""}"",""{""}"",""{""}"",""{""}"",""{"SQL Server"}"",""{configuracion.Servidor}"",""{configuracion.Puerto}"",""{configuracion.Usuario}"",""{configuracion.Password}"",""{"GOURMETPOS"}"");");


        }

        private void Cancelar(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }

        private void RecargaCadena(object sender, RoutedEventArgs e)
        {
            TxtCadenaCon.Text =
                Configuracion.BuildFrom(TxtDbName.Text, TxtContraseña.Text, TxtPuerto.Text, TxtServidor.Text, TxtUsuario.Text).CadenaCon;
        }

        private void ProbarConexion(object sender, RoutedEventArgs e)
        {
            this.Connection.ChangeConnectionString(TxtCadenaCon.Text);

            if (this.Connection.PruebaConexion() is Exception ex)
            {
                Plugin.Xamarin.Tools.Shared.Services.CustomMessageBox.Current.Show("La cadena de conexión no es valida." + Environment.NewLine + ex.Message, "La cadena de conexión no es valida.", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                TxtStatus.Text = ex.Message;
                ImgStatus.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\002-usb-1.png");
                return;
            }
            ImgStatus.CambiarImagen($"{Plugin.Xamarin.Tools.WPF.Tools.Instance.LibraryPath}\\Imgs\\001-usb.png");
            TxtStatus.Text = "Conexión exitosa";
            Plugin.Xamarin.Tools.Shared.Services.CustomMessageBox.Current.Show("Conexión correcta", "Todo listo", CustomMessageBoxButton.OK,
                CustomMessageBoxImage.Information);
        }

        private void SeleccionaEmpresa(object sender, SelectionChangedEventArgs e)
        {
            if (CmbxEmpresas.SelectedItem == null) return;



            string[] args = AppData.BaseDeDatosLite.CadenaCon(
                CmbxEmpresas.SelectedValue.ToString()).Replace(Environment.NewLine, "").Replace('\n', ' ')
                .Replace('\r', ' ').Split(';');


            TxtCadenaCon.Text = string.Join(";" + Environment.NewLine, (from w in args where !string.IsNullOrEmpty(w.Trim()) select w)).Trim();

            Configuracion cadena = AppData.BaseDeDatosLite.DatosCadenaCon(CmbxEmpresas.SelectedItem.ToString());
            TxtDbName.Text = cadena.NombreDB;
            TxtContraseña.Text = cadena.Password;
            TxtServidor.Text = cadena.Servidor;
            TxtUsuario.Text = cadena.Usuario;
            TxtPuerto.Text = cadena.Puerto;
        }
    }
}
