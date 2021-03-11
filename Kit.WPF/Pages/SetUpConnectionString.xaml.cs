using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
using Kit.Services.BarCode;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Kit.WPF.Controls;
using Kit.WPF.Extensions;
using Kit.WPF.Services;
using Kit.WPF.Services.ICustomMessageBox;
using Microsoft.Win32;
using ZXing;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Interaction logic for SetUpConnectionString.xaml
    /// </summary>
    public partial class SetUpConnectionString : ObservableWindow
    {
        private readonly Empresas Empresas;
        private readonly string DeviceId;
        private SQLServerConnection SqlServer;
        private readonly SQLiteConnection SqLite;
        public Configuracion _Configuration;
        public Configuracion Configuration
        {
            get => _Configuration;
            set
            {
                _Configuration = value;
                Raise(() => Configuration);
            }
        }

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

        private async void ImportarCadena(object sender, RoutedEventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();
            abrir.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (abrir.ShowDialog() ?? false)
            {
                var qr = new FileInfo(abrir.FileName);
                BarcodeDecoding reader = new BarcodeDecoding();
                Result result = await reader.Decode(qr, BarcodeFormat.QR_CODE
                    , new[]
                    {
                        new KeyValuePair<DecodeHintType, object>(DecodeHintType.TRY_HARDER,null)
                    });
                Deserialize(result?.Text);
            }
        }
        private void Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                Configuracion configuracion_qr = Configuracion.DeSerialize(json);
                if (configuracion_qr != null)
                {
                    this.Configuration = configuracion_qr;
                    //this.CargarCadena();
                }
                else
                {
                    Kit.WPF.Services.ICustomMessageBox.CustomMessageBox
                        .Show("Formato Qr incorrecto", "Incorrecto", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }
            }
        }

        private void CompartirCadena(object sender, RoutedEventArgs e)
        {
            string code = this.Configuration?.Serialize();
            if (!string.IsNullOrEmpty(code))
            {
                ShareCadenaCon share = new ShareCadenaCon(this.Configuration.NombreDB, code)
                {
                    Owner = this
                };
                share.ShowDialog();
            }
        }
    }

}
