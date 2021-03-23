using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kit.Sql.Sqlite;
using Kit.Model;

namespace Kit.CadenaConexion
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
        public string Seleccionada { get; set; }
        private readonly SQLiteConnection SQLHLite;
        public Empresas(SQLiteConnection SQLHLite)
        {
            this.SQLHLite = SQLHLite;
        }
        public IEnumerable<string> ListarEmpresas()
        {
            this.ListaEmpresas = SQLHLite.Table<Configuracion>().Select(x => x.Empresa).ToList();
            this.ListaEmpresas.Add(string.Empty);
            return this.ListaEmpresas;           
        }
        public Configuracion CadenaCon(string Empresa, string DeviceId)
        {
            Configuracion configuracion = null;
            try
            {
                configuracion = SQLHLite.Table<Configuracion>()
                    .Where(x => x.Empresa == Empresa).FirstOrDefault();
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
