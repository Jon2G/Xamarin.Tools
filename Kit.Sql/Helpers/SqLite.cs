using System;
using System.Collections.Generic;
//Alias para poder trabajar con mas facilidad
using SqlConnection = SQLite.SQLiteConnection;
using SqlCommand = SQLite.SQLiteCommand;
using SqlDataReader = Kit.Sql.SQLiteNetExtensions.ReaderItem;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using System.Text;
using System.Diagnostics;
using Kit.Sql.Readers;
using Kit.Sql.Interfaces;
using Kit.Sql.Reflection;
using System.Text.RegularExpressions;
namespace Kit.Sql.Helpers
{
    public class SqLite : BaseSQLHelper
    {
        private Type AssemblyType;
        private string ScriptResourceName;

        [Obsolete("Use ScriptResourceName")]
        public event EventHandler OnCreateDB;
        //private FileInfo file;
        public readonly string RutaDb;
        public readonly ulong DBVersion;
        public SqLite(ulong DBVersion, string DBName)
        {
            if (Sqlh.Instance is null)
            {
                throw new Exception("Please call SQLHelper.Init before using it");
            }

            FileInfo db = new FileInfo($"{Sqlh.Instance.LibraryPath}/{DBName}");
            RutaDb = db.FullName;
            this.DBVersion = DBVersion;
        }

        public SqLite SetDbScriptResource<T>(string ScriptResourceName)
        {
            AssemblyType = typeof(T);
            this.ScriptResourceName = ScriptResourceName;
            return this;
        }

        public SqLite(FileInfo file)
        {
            if (Sqlh.Instance is null)
            {
                throw new Exception("Please call SQLHelper.Initi(LibraryPath,Debugging); before using it");
            }
            file.Refresh();
            RutaDb = file.FullName;
        }

        /// <summary>
        /// Comprueba que la base de datos exista y que sea la versión mas reciente
        /// de lo contrario crea una nueva base de datos
        /// </summary>
        public SqLite RevisarBaseDatos()
        {
            FileInfo db = new FileInfo(RutaDb);

            if (!db.Exists)
            {
                Crear(Conecction());
            }
            ulong dbVersion = GetDbVersion();
            if (dbVersion != DBVersion)
            {
                db.Delete();
                Crear(Conecction());
            }
            return this;
        }
        private void RevisarTablaDbVersion(SqlConnection connection)
        {
            if (TableExists("DB_VERSION"))
            {
                return;
            }
            connection.Execute(@"CREATE TABLE DB_VERSION ( VERSION VARCHAR NOT NULL )");
            connection.Execute($"INSERT INTO DB_VERSION(VERSION) VALUES('{DBVersion}')");
        }
        private void RevisarTablaConfiguracion()
        {
            if (!TableExists("CONFIGURACION"))
            {
                EXEC(@"CREATE TABLE CONFIGURACION (
ID INTEGER IDENTITY(1,1) PRIMARY KEY, 
ID_DISPOSITIVO TEXT NOT NULL,
NOMBREDB TEXT,
NOMBRE TEXT,
SERVIDOR TEXT,
PUERTO TEXT,
USUARIO TEXT,
PASSWORD TEXT,
CADENA_CON TEXT NOT NULL);");
            }
            return;
        }


        public ulong GetDbVersion()
        {
            ulong dbVersion = 0;
            try
            {
                dbVersion = Single<ulong>("SELECT VERSION FROM DB_VERSION");
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return dbVersion;
        }
        private void Crear(SqlConnection connection)
        {
            if (TableExists("DB_VERSION"))
            {
                connection.Execute("DROP TABLE DB_VERSION");
            }
            RevisarTablaDbVersion(connection);
            RevisarTablaConfiguracion();
            if (AssemblyType != null && !string.IsNullOrEmpty(ScriptResourceName))
            {
                CreateDbFromScript(connection);
            }
            else
            {
                OnCreateDB?.Invoke(this, EventArgs.Empty);
            }
            connection.Close();
        }
        private void CreateDbFromScript(SqlConnection connection)
        {
            string sql = string.Empty;
            using (ReflectionCaller reflection = new ReflectionCaller())
            {
                sql = ReflectionCaller.ToText(reflection
                    .GetAssembly(AssemblyType)
                    .GetResource(ScriptResourceName));
            }
            if (!string.IsNullOrEmpty(sql))
            {
                Batch(connection, sql);
            }
            AssemblyType = null;
            ScriptResourceName = null;

        }

        public bool EliminarDB()
        {
            try
            {
                FileInfo file = new FileInfo(RutaDb);
                if (file.Exists)
                {
                    File.Delete(RutaDb);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
                return false;
            }
        }
        public SqlConnection Conecction()
        {
            SqlConnection con;
            try
            {
                con = new SqlConnection(RutaDb);
                return con;
            }
            catch (Exception ex)
            {
                con = null;
                Log.LogMe(ex);
            }
            return con;
        }
        public SQLiteAsyncConnection ConecctionAsync()
        {
            SQLiteAsyncConnection con;
            try
            {
                con = new SQLiteAsyncConnection(RutaDb);
                return con;
            }
            catch (Exception ex)
            {
                con = null;
                Log.LogMe(ex);
            }
            return con;
        }
        public int Querry(string sql, params object[] args)
        {
            int afectadas = -1;
            using (SqlConnection con = Conecction())
            {
                afectadas = con.Execute(sql, args);
                con.Close();
            }

            return afectadas;
        }
        public override T Single<T>(string sql)
        {
            T result = default;
            try
            {
                using (IReader reader = Read(sql))
                {
                    if (reader.Read())
                    {
                        result =
                            typeof(T).IsEnum ?
                                (T)Enum.Parse(typeof(T), reader[0].ToString(), true) :
                                (T)Convert.ChangeType(reader[0], typeof(T));
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogMe(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
        }
        public T Single<T>(SqlConnection con, string sql)
        {
            T result = default;
            try
            {
                using (IReader reader = Read(sql, con))
                {
                    if (reader.Read())
                    {
                        result =
                            typeof(T).IsEnum ?
                                (T)Enum.Parse(typeof(T), reader[0].ToString(), true) :
                                (T)Convert.ChangeType(reader[0], typeof(T));
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogMe(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
        }

        public override int EXEC(string sql)
        {
            return EXEC(sql);
        }
        public int EXEC(string sql, params object[] parametros)
        {
            Log.DebugMe(sql);
            int afectadas = -1;
            using (SqlConnection con = Conecction())
            {
                afectadas = con.Execute(sql, parametros);
                con.Close();
            }
            return afectadas;
        }
        public int EXEC(SqlConnection con, string sql, params object[] parametros)
        {
            Log.DebugMe(sql);
            return con.Execute(sql, parametros);
        }
        public T ExecuteRead<T>(string sql, params object[] parametros)
        {
            Log.DebugMe(sql);
            T result = default;
            using (SqlConnection con = Conecction())
            {
                result = con.ExecuteScalar<T>(sql, parametros);
                con.Close();
            }
            return result;
        }
        public List<T> Lista<T>(string sql)
        {
            List<T> result = new List<T>();
            using (IReader reader = Read(sql))
            {
                while (reader.Read())
                {
                    result.Add((T)Convert.ChangeType(reader[0], typeof(T)));
                }
            }
            return result;
        }
        //ListaTupla
        public List<Tuple<T, Q>> ListaTupla<T, Q>(string sql, params object[] parameters)
        {
            List<Tuple<T, Q>> result = new List<Tuple<T, Q>>();

            using (IReader reader = Read(sql))
            {
                while (reader.Read())
                {
                    result.Add(new Tuple<T, Q>
                        ((T)Convert.ChangeType(reader[0], typeof(T)),
                        (Q)Convert.ChangeType(reader[1], typeof(Q))));
                }
            }

            return result;
        }
        public DataTable DataTable(string Querry, string TableName = null)
        {
            DataTable result = new DataTable(TableName);
            using (LiteReader reader = (LiteReader)Read(Querry))
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Columns.Add(reader.Fields[i]);
                    }
                }
                else
                {
                    return result;
                }
                do
                {
                    List<object> row = new List<object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader[i]);
                    }

                    result.Rows.Add(row.ToArray());
                } while (reader.Read());
            }
            return result;
        }
        public override IReader Read(string sql)
        {
            try
            {
                return new LiteReader(Conecction(), sql);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al compilar y ejecutar un leector");
                return new FakeReader();
            }
        }


        public LiteReader Read(string sql, SqlConnection connection)
        {
            try
            {
                return new LiteReader(connection, sql);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al compilar y ejecutar un leector");
                return null;
            }
        }
        public bool Exists(string sql)
        {
            using (IReader reader = Read(sql))
            {
                return reader?.Read() ?? false;
            }
        }
        public override bool TableExists(string TableName)
        {
            using (IReader reader = Read($"SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}';"))
            {
                return reader?.Read() ?? false;
            }
        }
        public void Batch(string sql)
        {
            Batch(Conecction(), sql);
        }
        public void Batch(SqlConnection con, string sql)
        {
            StringBuilder sqlBatch = new StringBuilder();
            try
            {
                Regex regex = new Regex(@"^.*\(.*\);$/gms");
                regex.Split(sql);

                foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "--GO" || (sqlBatch.Length > 0 && EndsWith(sqlBatch, ");")))
                    {
                        string batch = sqlBatch.ToString();
                        //if (IsBalanced(batch))
                        //{
                        if (!string.IsNullOrEmpty(batch))
                            EXEC(batch);
                        sqlBatch.Clear();
                        //}
                    }
                    if (!line.StartsWith("--"))
                    {
                        sqlBatch.Append(line.Trim());
                    }
                }
                if (sqlBatch.Length > 0)
                {
                    EXEC(sqlBatch.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex.Message);
            }
        }

        private bool IsBalanced(string batch)
        {
            int open = batch.Count(x => x == '(');
            int closed = batch.Count(x => x == ')');

            return open == closed;
        }

        public bool EndsWith(StringBuilder sb, string test)
        {
            if (sb.Length < test.Length)
                return false;

            string end = sb.ToString(sb.Length - test.Length, test.Length);
            return end.Equals(test);
        }


        public int LastScopeIdentity(SqlConnection con)
        {
            return Single<int>(con, "SELECT last_insert_rowid();");
        }
    }
}
