using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Hjg.Pngcs;
using Kit.Entity;
using Kit.Model;



namespace Kit.SetUpConnectionString
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
        public IDbConnection SqlServer { get; set; }
        public readonly IDbConnection SqLite;
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

        public SetUpConnectionStringViewModelBase(IDbConnection SqLite, IDbConnection SqlServer, Configuracion configuracion)
        {
            this.SqLite = SqLite;
            this.SqlServer = SqlServer ?? new SqlConnection(String.Empty);
            this.Empresas = new Empresas(SqLite);
            this.Configuration = configuracion ?? new Configuracion();
            Configuration.IdentificadorDispositivo = DeviceId;
            this.Empresas.ListarEmpresas();
        }

        public Exception TestConnection()
        {
            SqlServer = new SqlConnection(this.Configuration.CadenaCon);
            if (!SqlServer.TryToConnect(out Exception ex))
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
            this.SqlServer = new SqlConnection(String.Empty);
        }
    }
}