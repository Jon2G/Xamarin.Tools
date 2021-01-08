using SQLHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.CadenaConexion
{
    public class Configuracion : ViewModelBase<Configuracion>
    {
        public string CadenaCon { get; set; }
        public string IdentificadorDispositivo { get; private set; }
        public string NombreDB { get; set; }
        public string Servidor { get; set; }
        public string Puerto { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Empresa { get; set; }
        public Configuracion(string CadenaCon, string DeviceId)
        {
            this.IdentificadorDispositivo = DeviceId;
            this.CadenaCon = CadenaCon;
        }
        public Configuracion(string NombreDB, string Servidor, string Puerto, string Usuario, string Password, string CadenaCon, string DeviceId)
        {
            this.NombreDB = NombreDB;
            this.Servidor = Servidor;
            this.Puerto = Puerto;
            this.Usuario = Usuario;
            this.Password = Password;
            this.IdentificadorDispositivo = DeviceId;
            this.CadenaCon = CadenaCon;
        }
        public static Configuracion ObtenerConfiguracion(SQLHLite SQLHLite, string DeviceId)
        {
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
                        Configuracion config = new Configuracion(
                            Convert.ToString(reader[0]),
                            Convert.ToString(reader[1]).Replace(@"\\", @"\"),
                            Convert.ToString(reader[2]),
                            Convert.ToString(reader[3]),
                            Convert.ToString(reader[4]),
                            Convert.ToString(reader[5]).Replace(@"\\", @"\"), DeviceId);
                        if (string.IsNullOrEmpty(config.CadenaCon))
                        {
                            config= BuildFrom(config);
                        }
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al recuperar la configuración");
            }
            return new Configuracion(string.Empty, string.Empty);
        }
        public static bool IsUserDefined(SQLHLite SQLHLite)
        {
            string pin = SQLHLite.Single<string>("SELECT DEFINED_USER_PIN FROM CONFIGURACION");
            return (!string.IsNullOrEmpty(pin));
        }

        public Exception ProbarConexion(SQLH SQLH)
        {
            Exception resultado = null;
            // UserDialogs.Instance.ShowLoading("Intentando conectar...", MaskType.Black);
            resultado = SQLH.PruebaConexion();
            //UserDialogs.Instance.HideLoading();
            return resultado;
        }
        public static Configuracion PorDefecto()
        {
            return new Configuracion(string.Empty, string.Empty);
        }
        public void Salvar(SQLHLite SQLHLite, SQLH SQLH)
        {
            try
            {
                SQLHLite.RevisarBaseDatos();
                SQLH.ChangeConnectionString(this.CadenaCon);
                bool existeRegistro = SQLHLite.Exists("SELECT *FROM CONFIGURACION");
                SQLHLite.EXEC(
                    existeRegistro ?
                        "UPDATE CONFIGURACION SET NOMBRE=?,NOMBREDB=?,SERVIDOR=?,PUERTO=?,USUARIO=?,PASSWORD=?,CADENA_CON=?,ID_DISPOSITIVO=?" :
                        "INSERT INTO CONFIGURACION (NOMBRE,NOMBREDB,SERVIDOR,PUERTO,USUARIO,PASSWORD,CADENA_CON,ID_DISPOSITIVO) VALUES(?,?,?,?,?,?,?,?)"
                        , this.Empresa, this.NombreDB, this.Servidor, this.Puerto, this.Usuario, this.Password, this.CadenaCon
                        , this.IdentificadorDispositivo);

                if (SQLH.TableExists("COMANDERAS_MOVILES"))
                {
                    existeRegistro = SQLH.Exists(
                        "SELECT *FROM COMANDERAS_MOVILES WHERE ID_DIPOSITIVO=@ID", false,
                        new SqlParameter("ID", this.IdentificadorDispositivo));
                    if (!existeRegistro)
                    {
                        SQLH.EXEC(
                            "INSERT INTO COMANDERAS_MOVILES (ID_DIPOSITIVO) VALUES (@ID);", System.Data.CommandType.Text, false,
                            new SqlParameter("ID", this.IdentificadorDispositivo));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            //finally
            //{
            //    Demon.Current.Awake();
            //}
        }
        public static Configuracion BuildFrom(Configuracion configuracion)
        {
            return BuildFrom(configuracion.NombreDB, configuracion.Password, configuracion.Puerto, configuracion.Servidor, configuracion.Usuario, configuracion.IdentificadorDispositivo);
        }
        public static Configuracion BuildFrom(
            string NombreDB = "", string Password = "",
            string Puerto = "", string Servidor = "",
            string Usuario = "", string DeviceId = "")
        {
            StringBuilder ConnectionString = new StringBuilder();
            ConnectionString.Append("Data Source=TCP:")
                .Append(Servidor)
                .Append((!string.IsNullOrEmpty(Puerto.Trim()) ? "," + Puerto : ""))//no puerto no lo pongo
                .Append(";Initial Catalog=")
                .Append(NombreDB);
            if (string.IsNullOrEmpty(Usuario.Trim()) && string.IsNullOrEmpty(Password.Trim()))//no usuario no contraseña credenciales por defecto
            {
                ConnectionString.Append(";Integrated Security=True;");
            }
            else
            {
                ConnectionString.Append(";Integrated Security=False;Persist Security Info=True;User ID=")
                    .Append(Usuario)
                    .Append(";Password=")
                    .Append(Password).Append(";");
            }

            string[] args = ConnectionString.
                Replace(Environment.NewLine, "").
                Replace('\n', ' ').
                Replace('\r', ' ').ToString().
                Split(';');
            return new Configuracion(string.Join(";" + Environment.NewLine, (from w in args where !string.IsNullOrEmpty(w.Trim()) select w)).Trim(), DeviceId)
            {
                NombreDB = NombreDB,
                Password = Password,
                Puerto = Puerto,
                Servidor = Servidor,
                Usuario = Usuario,

            };
        }
    }
}
