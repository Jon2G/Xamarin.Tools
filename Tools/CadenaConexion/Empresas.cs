using SQLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools.CadenaConexion
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
            using (var reader = SQLHLite.Leector("SELECT Nombre FROM configuracion"))
            {
                while (reader.Read())
                {
                    empresas.Add(reader[0].ToString());
                }
            }
            return empresas;

        }
        public Configuracion CadenaCon(string Empresa,string DeviceId)
        {
            Configuracion configuracion = null;

            try
            {
                if (!File.Exists(SQLHLite.RutaDb))
                {
                    SQLHLite.RevisarBaseDatos();
                }
                using (IReader reader = SQLHLite.Leector(
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
                            Convert.ToString(reader[5]).Replace(@"\\", @"\"),DeviceId);
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
