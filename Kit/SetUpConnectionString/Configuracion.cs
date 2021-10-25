using System;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Newtonsoft.Json;

namespace Kit.SetUpConnectionString
{
    [Preserve(AllMembers = true), Table("CONFIGURACION"), SyncMode(SyncDirection.NoSync)]
    public class Configuracion : ModelBase, ICloneable
    {
        private string _cadenaCon;
        private string _nombreDb;
        private string _servidor;
        private string _puerto;
        private string _usuario;
        private string _password;
        private string _empresa;

        [NotNull]
        public string IdentificadorDispositivo { get; set; }

        [NotNull]
        public string CadenaCon
        {
            get => _cadenaCon;
            set
            {
                _cadenaCon = value;
                Raise(() => CadenaCon);
            }
        }

        public string NombreDB
        {
            get => _nombreDb;
            set
            {
                if (String.CompareOrdinal(_nombreDb, value) != 0)
                {
                    _nombreDb = value;
                    RefreshConnectionString();
                    Raise(() => NombreDB);
                }
            }
        }

        public static Configuracion FromMyB(string connectionString)
        {
            string Catalog = Regex.Match(connectionString, "(Catalog +?=(?<Value>.+?);)").TryGetGroup("Value")?.Value;
            string DataSource = Regex.Match(connectionString, "(Data Source=(?<Value>.+?);)").TryGetGroup("Value")?.Value
                ?.Replace("TCP:", string.Empty);
            string UserId = Regex.Match(connectionString, "(User ID=(?<Value>.+?);)").TryGetGroup("Value")?.Value;
            string Password = Regex.Match(connectionString, "(Password=(?<Value>.+);?)").TryGetGroup("Value")?.Value;
            return Configuracion.BuildFrom(NombreDB: Catalog, Password: Password, Servidor: DataSource, Usuario: UserId);
        }

        public string Servidor
        {
            get => _servidor;
            set
            {
                if (String.CompareOrdinal(_servidor, value) != 0)
                {
                    _servidor = value;
                    RefreshConnectionString();
                    Raise(() => Servidor);
                }
            }
        }

        public string Puerto
        {
            get => _puerto;
            set
            {
                if (String.CompareOrdinal(_puerto, value) != 0)
                {
                    _puerto = value;
                    RefreshConnectionString();
                    Raise(() => Puerto);
                }
            }
        }

        public string Usuario
        {
            get => _usuario;
            set
            {
                if (String.CompareOrdinal(_usuario, value) != 0)
                {
                    _usuario = value;
                    RefreshConnectionString();
                    Raise(() => Usuario);
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (String.CompareOrdinal(_password, value) != 0)
                {
                    _password = value;
                    RefreshConnectionString();
                    Raise(() => Password);
                }
            }
        }

        [PrimaryKey, MaxLength(50)]
        public string Empresa
        {
            get => _empresa;
            set
            {
                if (String.CompareOrdinal(_empresa, value) != 0)
                {
                    _empresa = value;
                    RefreshConnectionString();
                    Raise(() => Empresa);
                }
            }
        }

        public bool Activa { get; set; }

        /// <summary>
        /// WARNING  SOLO PARA REFLEXIÓN XML
        /// </summary>
        public Configuracion() { }

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

        public static Configuracion ObtenerConfiguracion(SQLiteConnection SQHLite, string DeviceId)
        {
            Configuracion configuracion = null;
            try
            {
                var configs = SQHLite.Table<Configuracion>();
                configuracion = configs.FirstOrDefault(x => x.Activa)
                ?? new Configuracion(string.Empty, DeviceId);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al obtener la configuracion");
            }
            return configuracion;
        }

        public static bool IsUserDefined(SQLiteConnection SQLHLite)
        {
            string pin = SQLHLite.ExecuteScalar<string>("SELECT DEFINED_USER_PIN FROM CONFIGURACION WHERE ACTIVA=1");
            return (!string.IsNullOrEmpty(pin));
        }

        public Exception ProbarConexion(SQLServerConnection SQLH)
        {
            Exception resultado = SQLH.TestConnection();
            if (resultado != null)
            {
                Log.Logger.Debug(resultado, "Prueba de conexión");
            }
            return resultado;
        }

        public static Configuracion PorDefecto()
        {
            return new Configuracion(string.Empty, string.Empty);
        }

        public void Salvar(SQLiteConnection SQLHLite, SQLServerConnection SQLH)
        {
            try
            {
                if (string.IsNullOrEmpty(this.Empresa))
                {
                    this.Empresa = NombreDB;
                }
                SQLH.ConnectionString = (new SqlConnectionStringBuilder(this.CadenaCon));
                this.Activa = true;
                SQLHLite.EXEC("UPDATE CONFIGURACION SET ACTIVA=0");
                SQLHLite.InsertOrReplace(this);

                if (SQLH.TableExists("COMANDERAS_MOVILES"))
                {
                    bool existeRegistro = SQLH.Exists(
                        "SELECT *FROM COMANDERAS_MOVILES WHERE ID_DIPOSITIVO=@ID",
                        new SqlParameter("ID", this.IdentificadorDispositivo));
                    if (!existeRegistro)
                    {
                        SQLH.Execute(
                            "INSERT INTO COMANDERAS_MOVILES (ID_DIPOSITIVO) VALUES (@ID);",
                            new SqlParameter("ID", this.IdentificadorDispositivo));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al salvar la configuración");
            }
        }

        public static Configuracion BuildFrom(Configuracion configuracion)
        {
            return BuildFrom(configuracion.NombreDB, configuracion.Password, configuracion.Puerto, configuracion.Servidor, configuracion.Usuario, configuracion.IdentificadorDispositivo);
        }

        public void RefreshConnectionString()
        {
            StringBuilder ConnectionString = new StringBuilder();
            ConnectionString.Append("Data Source=TCP:")
                .Append(Servidor)
                .Append((!string.IsNullOrEmpty(Puerto?.Trim()) ? "," + Puerto : ""))//no puerto no lo pongo
                .Append(";Initial Catalog=")
                .Append(NombreDB);
            if (string.IsNullOrEmpty(Usuario?.Trim()) && string.IsNullOrEmpty(Password?.Trim()))//no usuario no contraseña credenciales por defecto
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

            this.CadenaCon = string.Join(";" + Environment.NewLine, args);
        }

        public static Configuracion BuildFrom(
            string NombreDB = "", string Password = "",
            string Puerto = "", string Servidor = "",
            string Usuario = "", string DeviceId = "")
        {
            return new Configuracion(string.Empty, DeviceId)
            {
                NombreDB = NombreDB,
                Password = Password,
                Puerto = Puerto,
                Servidor = Servidor,
                Usuario = Usuario,
            };
        }

        public string Serialize() => JsonConvert.SerializeObject(this);

        public static Configuracion DeSerialize(string codigoBarras)
        {
            Configuracion configuracion = null;
            try
            {
                if (string.IsNullOrEmpty(codigoBarras))
                {
                    return null;
                }
                configuracion = JsonConvert.DeserializeObject<Configuracion>(codigoBarras);
                return configuracion;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "While deserializing the connection string qr");
                return null;
            }
        }

        public T Clone<T>() => (T)Clone();
        public object Clone() => DeSerialize(this.Serialize());
    }
}