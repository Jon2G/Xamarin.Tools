using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using Kit.CadenaConexion;
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Sqlite;
using Kit.WPF.Services;
using Kit.WPF.Services.ICustomMessageBox;
using SQLServer;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Interaction logic for SetUpConnectionString.xaml
    /// </summary>
    public partial class SetUpConnectionString : Window
    {
        private readonly Empresas Empresas;
        private readonly string DeviceId;
        private SQLServerConnection SqlServer;
        private readonly SQLiteConnection SqLite;
        public Configuracion Configuration { get; set; }

        public SetUpConnectionString(Exception ex, Configuracion Configuration, SQLiteConnection SqLite = null)
        {
            ChangeDataContext(Configuration);
            this.SqLite = SqLite;
            DeviceId = new Kit.WPF.Services.DeviceInfo().DeviceId;
            if (SqLite != null)
                Empresas = new Empresas(SqLite);
            InitializeComponent();
            TxtStatus.Text = ex.Message;
        }
        private void ChangeDataContext(Configuracion Configuration)
        {
            this.Configuration = Configuration;
            this.Configuration.IdentificadorDispositivo = Device.Current.DeviceId;
            this.DataContext = this.Configuration;
        }

        public SetUpConnectionString(SQLServerConnection SqlServer, SQLiteConnection SqLite = null, Exception ex = null)
        {
            ChangeDataContext(new Configuracion());
            this.SqlServer = SqlServer;
            this.SqLite = SqLite;
            DeviceId = new Kit.WPF.Services.DeviceInfo().DeviceId;
            if (SqLite != null)
                Empresas = new Empresas(SqLite);
            InitializeComponent();
            TxtStatus.Text = ex?.Message ?? "La cadena de conexion es correcta";
        }

        //private void CadenaCon_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    this.Configuration.CadenaCon = SqlServer.ConnectionString;

        //    CmbxEmpresas.ItemsSource = Empresas.ListarEmpresas();
        //}

        private async void Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlServer = new SQLServerConnection(this.Configuration.CadenaCon);
                SqlServer.ConnectionString = new SqlConnectionStringBuilder(this.Configuration.CadenaCon);
                if (SqlServer.TestConnection(this.Configuration.CadenaCon) is Exception ex)
                {
                    if (CustomMessageBox.ShowYesNo(
                            "La conexión actual no es valida\n" + ex.Message + "\n¿Desea guardarla de todas formas?",
                            "La conexión no es valida", "Si", "No", CustomMessageBoxImage.Question) == CustomMessageBoxResult.No)
                    {
                        return;
                    }
                }
                SqlServer.ConnectionString = new SqlConnectionStringBuilder(this.Configuration.CadenaCon);
                if (this.SqLite != null)
                {
                    this.Configuration.Salvar(SqLite, SqlServer);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al  guardar la cadena de conexión");
            }
        }

        private void Cancelar(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }



        private void ProbarConexion(object sender, RoutedEventArgs e)
        {
            SqlServer = new SQLServerConnection(this.Configuration.CadenaCon);
            if (SqlServer.TestConnection() is Exception ex)
            {
                CustomMessageBox.Show("La cadena de conexión no es valida." + Environment.NewLine + ex.Message, "La cadena de conexión no es valida.", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                TxtStatus.Text = ex.Message;
                return;
            }
            TxtStatus.Text = "Conexión exitosa";
            CustomMessageBox.Show("Conexión correcta", "Todo listo", CustomMessageBoxButton.OK,
                CustomMessageBoxImage.Information);
        }

        private void SeleccionaEmpresa(object sender, SelectionChangedEventArgs e)
        {
            if (CmbxEmpresas.SelectedItem == null) return;
            string[] args = Empresas.CadenaCon(
                CmbxEmpresas.SelectedValue.ToString(), DeviceId).CadenaCon.Replace(Environment.NewLine, "").Replace('\n', ' ')
                .Replace('\r', ' ').Split(';');
            this.Configuration.CadenaCon = string.Join(";" + Environment.NewLine, (from w in args where !string.IsNullOrEmpty(w.Trim()) select w)).Trim();

            ChangeDataContext(Empresas.CadenaCon(CmbxEmpresas.SelectedItem.ToString(), DeviceId));
        }
    }

}
