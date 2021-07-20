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
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Services.BarCode;
using Kit.SetUpConnectionString;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Kit.WPF.Controls;
using Kit.WPF.Dialogs.ICustomMessageBox;
using Kit.WPF.Extensions;
using Kit.WPF.Services;
using Microsoft.Win32;
using ZXing;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Interaction logic for SetUpConnectionString.xaml
    /// </summary>
    public partial class SetUpConnectionString : ObservableWindow
    {
        public SetUpConnectionStringViewModelBase Model { get; set; }

        public SetUpConnectionString(Exception ex, SQLServerConnection SqlServer, SQLiteConnection SqLite, Configuracion Configuration)
        {
            this.Model = new SetUpConnectionStringViewModelBase(SqLite, SqlServer, Configuration);
            InitializeComponent();
            TxtStatus.Text = ex?.Message ?? "La cadena de conexion es correcta";
        }

        public SetUpConnectionString(Exception ex, Configuracion Configuration, SQLiteConnection SqLite = null) :
            this(ex, null, SqLite, Configuration)
        {
            TxtStatus.Text = ex?.Message ?? "La cadena de conexion es correcta";
        }

        public SetUpConnectionString(SQLServerConnection SqlServer, SQLiteConnection SqLite = null, Exception ex = null) :
            this(ex, SqlServer, SqLite, new Configuracion())
        {
            TxtStatus.Text = ex?.Message ?? "La cadena de conexion es correcta";
        }

        private void Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.Model.Configuration.Empresa))
                {
                    CustomMessageBox.Show("Debe indicar un nombre para esta cadena.", "La conexión no es valida", CustomMessageBoxButton.OK, CustomMessageBoxImage.Warning);
                    return;
                }

                var SqlServer = new SQLServerConnection(this.Model.ConnectionString);
                SqlServer.ConnectionString = new SqlConnectionStringBuilder(this.Model.ConnectionString);
                if (SqlServer.TestConnection(this.Model.ConnectionString) is Exception ex)
                {
                    if (CustomMessageBox.ShowYesNo(
                            "La conexión actual no es valida\n" + ex.Message + "\n¿Desea guardarla de todas formas?",
                            "La conexión no es valida", "Si", "No", CustomMessageBoxImage.Question) == CustomMessageBoxResult.No)
                    {
                        return;
                    }
                }
                SqlServer.ConnectionString = new SqlConnectionStringBuilder(this.Model.ConnectionString);
                this.Model.SqlServer = SqlServer;
                this.Model.Save();
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
            if (this.Model.TestConnection() is Exception ex)
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
            if (string.IsNullOrEmpty(this.Model.Empresas.Seleccionada))
            {
                this.Model.Clear();
                return;
            }
            this.Model.FromEmpresa(this.Model.Empresas.Seleccionada);
        }

        private void ImportarCadena(object sender, RoutedEventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();
            abrir.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (abrir.ShowDialog() ?? false)
            {
                var qr = new FileInfo(abrir.FileName);
                BarcodeDecoding reader = new BarcodeDecoding();
                Result result = reader.Decode(qr, BarcodeFormat.QR_CODE
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
                    if (string.IsNullOrEmpty(configuracion_qr.Empresa))
                    {
                        configuracion_qr.Empresa = configuracion_qr.NombreDB;
                    }
                    this.Model.Configuration = configuracion_qr;
                }
                else
                {
                    CustomMessageBox
                        .Show("Formato Qr incorrecto", "Incorrecto", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                }
            }
        }

        private void CompartirCadena(object sender, RoutedEventArgs e)
        {
            string code = this.Model.Configuration?.Serialize();
            if (!string.IsNullOrEmpty(code))
            {
                ShareCadenaCon share = new ShareCadenaCon(this.Model.Configuration.NombreDB, code)
                {
                    Owner = this
                };
                share.ShowDialog();
            }
        }
    }
}