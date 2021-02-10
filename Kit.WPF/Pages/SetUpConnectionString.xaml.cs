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
using Kit.CadenaConexion;
using Kit.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.WPF.Services;
using Kit.WPF.Services.ICustomMessageBox;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Interaction logic for SetUpConnectionString.xaml
    /// </summary>
    public partial class SetUpConnectionString : Window
    {
        private readonly Empresas Empresas;
        private readonly string DeviceId;
        private SqlServer SqlServer;
        private readonly SqLite SqLite;
        public Configuracion Configuration { get; set; }

        public SetUpConnectionString(Exception ex, Configuracion Configuration, Sql.Helpers.SqLite SqLite = null)
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
            this.DataContext = this.Configuration;
        }

        public SetUpConnectionString(SqlServer SqlServer, Sql.Helpers.SqLite SqLite = null)
        {
            this.SqlServer = SqlServer;
            this.SqLite = SqLite;
            DeviceId = new Kit.WPF.Services.DeviceInfo().DeviceId;
            if (SqLite != null)
                Empresas = new Empresas(SqLite);
            InitializeComponent();
            TxtStatus.Text = "La cadena de conexion es correcta";
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
                SqlServer = new SqlServer(this.Configuration.CadenaCon);
                SqlServer.ChangeConnectionString(this.Configuration.CadenaCon);
                if (SqlServer.PruebaConexion(this.Configuration.CadenaCon) is Exception ex)
                {
                    if (CustomMessageBox.ShowYesNo(
                            "La conexión actual no es valida\n" + ex.Message + "\n¿Desea guardarla de todas formas?",
                            "La conexión no es valida", "Si", "No", CustomMessageBoxImage.Question) == CustomMessageBoxResult.No)
                    {
                        return;
                    }
                }
                SqlServer.ChangeConnectionString(this.Configuration.CadenaCon);
                if (this.SqLite != null)
                {
                    this.Configuration.Salvar(SqLite, SqlServer);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
        }

        private void Cancelar(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }



        private void ProbarConexion(object sender, RoutedEventArgs e)
        {
            SqlServer = new SqlServer(this.Configuration.CadenaCon);
            SqlServer.ChangeConnectionString(this.Configuration.CadenaCon);
            if (SqlServer.PruebaConexion() is Exception ex)
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
