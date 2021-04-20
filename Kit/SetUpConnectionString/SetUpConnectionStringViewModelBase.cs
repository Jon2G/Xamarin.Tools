using Kit.Model;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}