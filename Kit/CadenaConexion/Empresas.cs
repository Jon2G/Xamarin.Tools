using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kit.Sql.Sqlite;

namespace Kit.CadenaConexion
{
    public class Empresas
    {
        private readonly SQLiteConnection SQLHLite;
        public Empresas(SQLiteConnection SQLHLite)
        {
            this.SQLHLite = SQLHLite;
        }
        public IEnumerable<string> ListarEmpresas()
        {
            return SQLHLite.Table<Configuracion>().Select(x => x.Empresa);

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
