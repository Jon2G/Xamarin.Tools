using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kit.CadenaConexion
{
    public class Empresas
    {
        private readonly SQLHLite SQLHLite;
        public Empresas(SQLHLite SQLHLite)
        {
            this.SQLHLite = SQLHLite;
        }
        public List<string> ListarEmpresas()
        {
            List<string> empresas = new List<string>();
            using (var reader = SQLHLite.Read("SELECT Nombre FROM configuracion"))
            {
                while (reader.Read())
                {
                    empresas.Add(reader[0]?.ToString() ?? $"EMP{empresas.Count}");
                }
            }
            return empresas;

        }
        public Configuracion CadenaCon(string Empresa, string DeviceId)
        {
            Configuracion configuracion = null;

            try
            {
                if (!File.Exists(SQLHLite.RutaDb))
                {
                    SQLHLite.RevisarBaseDatos();
                }
                using (IReader reader = SQLHLite.Read(
                    "SELECT NOMBREDB,SERVIDOR,PUERTO,USUARIO,PASSWORD,CADENA_CON FROM CONFIGURACION"))
                {
                    if (reader.Read())
                    {
                        return new Configuracion(
                            Convert.ToString(reader[0]),
                            Convert.ToString(reader[1]).Replace(@"\\", @"\"),
                            Convert.ToString(reader[2]),
                            Convert.ToString(reader[3]),
                            Convert.ToString(reader[4]),
                            Convert.ToString(reader[5]).Replace(@"\\", @"\"), DeviceId);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al recuperar la configuración");
            }
            return configuracion;
        }
    }
}
