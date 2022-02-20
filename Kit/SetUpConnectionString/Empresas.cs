using System;
using System.Collections.Generic;
using System.Linq;
using Kit.Model;
using Kit.Sql.Sqlite;

namespace Kit.SetUpConnectionString
{
    public class Empresas : ModelBase
    {
        public List<string> _ListaEmpresas;
        public List<string> ListaEmpresas
        {
            get => _ListaEmpresas; set
            {
                _ListaEmpresas = value;
                Raise(() => ListaEmpresas);
            }
        }

        public string _Seleccionada;
        public string Seleccionada
        {
            get => _Seleccionada;
            set { _Seleccionada = value; Raise(() => Seleccionada); }
        }
        private readonly SQLiteConnection SQLHLite;
        public Empresas(SQLiteConnection SQLHLite)
        {
            this.SQLHLite = SQLHLite;
        }
        public IEnumerable<string> ListarEmpresas() => ListarEmpresas(SQLHLite);
        public IEnumerable<string> ListarEmpresas(SQLiteConnection lite)
        {
            if (ListaEmpresas is null)
                ListaEmpresas = new List<string>();
            if (lite is null)
                return this.ListaEmpresas;
                this.ListaEmpresas.AddRange(lite.Table<Configuracion>().Select(x => x.Empresa));
            if (this.ListaEmpresas.Any())
            {
                this.Seleccionada = lite.Table<Configuracion>().Where(x => x.Activa).Select(x => x.Empresa).FirstOrDefault();
            }
            this.ListaEmpresas.Add(string.Empty);
            return this.ListaEmpresas;
        }
        public Configuracion CadenaCon(string Empresa, string DeviceId)
        {
            Configuracion configuracion = null;
            try
            {
                configuracion = SQLHLite
                    .Table<Configuracion>().FirstOrDefault(x => x.Empresa == Empresa);
                if (configuracion != null)
                {
                    configuracion.IdentificadorDispositivo = DeviceId;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al recuperar la configuración");
            }

            return configuracion;
        }
    }
}
