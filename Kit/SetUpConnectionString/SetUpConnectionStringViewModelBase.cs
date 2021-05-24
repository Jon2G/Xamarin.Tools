using Kit.Model;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Kit.Extensions;

namespace Kit.CadenaConexion
{
    public class SetUpConnectionStringViewModelBase : ModelBase
    {
        public Empresas _Empresas { get; set; }

        public Empresas Empresas
        {
            get => _Empresas; set
            {
                {
                    _Empresas = value;
                    Raise(() => Empresas);
                }
            }
        }

        public string DeviceId => Daemon.Devices.Device.Current.DeviceId;
        public SQLServerConnection SqlServer { get; set; }
        public readonly SQLiteConnection SqLite;
        private Configuracion _Configuration;

        public Configuracion Configuration
        {
            get => _Configuration;
            set
            {
                _Configuration = value;
                Raise(() => Configuration);
                Raise(() => ConnectionString);
            }
        }

        public string ConnectionString => this.Configuration?.CadenaCon;

        public SetUpConnectionStringViewModelBase(SQLiteConnection SqLite, SQLServerConnection SqlServer, Configuracion configuracion)
        {
            this.SqLite = SqLite;
            this.SqlServer = SqlServer ?? new SQLServerConnection(String.Empty);
            this.Empresas = new Empresas(SqLite);
            this.Configuration = Configuration ?? new Configuracion();
            Configuration.IdentificadorDispositivo = DeviceId;
            this.Empresas.ListarEmpresas();

            this.ServerSuggestions = new ObservableCollection<string>();
            this.PortSuggestions = new ObservableCollection<string>();
            this.DatabaseSuggestions = new ObservableCollection<string>();
            this.SuggestServerCommand = new Command(SuggestServer);
            this.SuggestPortCommand = new Command(SuggestPort);
            this.SuggestDatabaseCommand = new Command(SuggestDatabase);
        }



        public Exception TestConnection()
        {
            SqlServer = new SQLServerConnection(this.Configuration.CadenaCon);
            if (SqlServer.TestConnection() is Exception ex)
            {
                return ex;
            }
            return null;
        }

        public void FromEmpresa(string name)
        {
            string[] args = Empresas.CadenaCon(name, DeviceId).CadenaCon.Replace(Environment.NewLine, "").Replace('\n', ' ')
                .Replace('\r', ' ').Split(';');
            this.Configuration.CadenaCon = string.Join(";" + Environment.NewLine, (from w in args where !string.IsNullOrEmpty(w.Trim()) select w)).Trim();
            this.Configuration = (Empresas.CadenaCon(name, DeviceId));
        }

        public void Save()
        {
            if (this.SqLite != null)
            {
                this.Configuration.Salvar(SqLite, SqlServer);
            }
        }

        public void Clear()
        {
            this.Configuration = new Configuracion()
            {
                IdentificadorDispositivo = DeviceId
            };
            this.SqlServer = new SQLServerConnection(String.Empty);
        }

        public ObservableCollection<string> ServerSuggestions { get; set; }
        public ObservableCollection<string> PortSuggestions { get; set; }
        public ObservableCollection<string> DatabaseSuggestions { get; set; }
   
        public ICommand SuggestServerCommand { get; }
        public ICommand SuggestPortCommand { get; }
        public ICommand SuggestDatabaseCommand { get; }
        private async void SuggestDatabase(object obj)
        {
            await Task.Yield();
            if (!this.DatabaseSuggestions.Any())
            {
                try
                {
                    using (SQLServerConnection con=new SQLServerConnection(this.ConnectionString))
                    {
                     this.DatabaseSuggestions.AddRange(con.GetDatabasesNames());
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e,"Leyendo las bases de datos");
                }
            }
        }
        private void SuggestPort()
        {
            this.PortSuggestions.Clear();
            this.PortSuggestions.Add("1433");
            this.PortSuggestions.Add("53100");
        }
        private void SuggestServer()
        {
            string  value= this.Configuration.Servidor;
            this.ServerSuggestions.Clear();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if ("192.168".StartsWith(value))
            {
                this.ServerSuggestions.Add("192.168.");
                value = "192.168.";
            }

            int last_dot = value.LastIndexOf('.');
            if (last_dot <= 0)
            {
                last_dot = 0;
            }

            string number = value.Substring(last_dot, value.Length - last_dot);
            int positions = 3-number.Length;

            if (positions > 0)
            {
                for (int j = 0; j < 10; j++)
                {
                    this.ServerSuggestions.Add($"{value}{j}");
                }
            }
            else
            {
                this.ServerSuggestions.Add($"{value}\\SQLEXPRESS");
                this.ServerSuggestions.Add($"{value}\\SQLEXPRESS01");
            }
            Raise(()=>ServerSuggestions);

        }
    }
}