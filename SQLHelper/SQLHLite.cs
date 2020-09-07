using System;
using System.Collections.Generic;
//Alias para poder trabajar con mas facilidad
using SqlConnection = SQLite.SQLiteConnection;
using SqlCommand = SQLite.SQLiteCommand;
using SqlDataReader = SQLite_Net.Extensions.Readers.ReaderItem;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLite_Net.Extensions.Readers;
using System.Text;
using System.Diagnostics;
using SQLHelper.Readers;
using SQLHelper.Interfaces;

namespace SQLHelper
{
    public class SQLHLite : BaseSQLHelper
    {
        public EventHandler OnCreateDB;
        //private FileInfo file;
        public readonly string RutaDb;
        public readonly string DBVersion;
        public SQLHLite(string DBVersion, string DBName)
        {
            if (SQLHelper.Instance is null)
            {
                throw new Exception("Please call SQLHelper.Initi(LibraryPath,Debugging); before using it");
            }
            FileInfo db = new FileInfo(Path.Combine(SQLHelper.Instance.LibraryPath, DBName));
            this.RutaDb = db.FullName;
            this.DBVersion = DBVersion;
        }

        public SQLHLite(FileInfo file)
        {
            if (SQLHelper.Instance is null)
            {
                throw new Exception("Please call SQLHelper.Initi(LibraryPath,Debugging); before using it");
            }
            file.Refresh();
            this.RutaDb = file.FullName;
        }

        /// <summary>
        /// Comprueba que la base de datos exista y que sea la versión mas reciente
        /// de lo contrario crea una nueva base de datos
        /// </summary>
        public void RevisarBaseDatos()
        {
            FileInfo db = new FileInfo(this.RutaDb);
            if (!db.Exists)
            {
                Crear(Conecction());
            }
            string dbVersion = GetDbVersion();
            if (dbVersion != DBVersion)
            {
                db.Delete();
                Crear(Conecction());
            }
        }

        public string GetDbVersion()
        {
            string dbVersion = null;
            try
            {
                using (IReader reader = new LiteReader(Conecction(), "SELECT VERSION FROM DB_VERSION"))
                {
                    if (reader.Read())
                    {
                        dbVersion = reader[0].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return dbVersion;
        }
        private void Crear(SqlConnection connection)
        {
            connection.Execute("DROP TABLE IF EXISTS DB_VERSION");
            connection.Execute(@"CREATE TABLE DB_VERSION ( VERSION VARCHAR NOT NULL )");
            connection.Execute($"INSERT INTO DB_VERSION(VERSION) VALUES('{DBVersion}')");
            OnCreateDB?.Invoke(connection, EventArgs.Empty);
            connection.Close();
        }
        public bool EliminarDB()
        {
            try
            {
                File.Delete(RutaDb);
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
                con = new SqlConnection(this.RutaDb);
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
            using (SqlConnection con = this.Conecction())
            {
                afectadas = con.Execute(sql, args);
                con.Close();
            }

            return afectadas;
        }
        public T Single<T>(string sql)
        {
            T result = default;
            try
            {
                using (IReader reader = Leector(sql))
                {
                    if (reader.Read())
                    {
                        result = (
                            typeof(T).IsEnum ?
                                (T)Enum.Parse(typeof(T), reader[0].ToString(), true) :
                                (T)Convert.ChangeType(reader[0], typeof(T)));
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogMe(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
        }
        public T Single<T>(SQLiteConnection con, string sql)
        {
            T result = default;
            try
            {
                using (IReader reader = Leector(sql, con))
                {
                    if (reader.Read())
                    {
                        result = (
                            typeof(T).IsEnum ?
                                (T)Enum.Parse(typeof(T), reader[0].ToString(), true) :
                                (T)Convert.ChangeType(reader[0], typeof(T)));
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogMe(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
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
        public int EXEC(SQLiteConnection con, string sql, params object[] parametros)
        {
            Log.DebugMe(sql);
            return con.Execute(sql, parametros);
        }
        public T ExecuteRead<T>(string sql, params object[] parametros)
        {
            Log.DebugMe(sql);
            T result = default(T);
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
            using (IReader reader = Leector(sql))
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

            using (IReader reader = Leector(sql))
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
            using (LiteReader reader = (LiteReader)Leector(Querry))
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Columns.Add(reader.Fields[i]);
                    }
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
        public IReader Leector(string sql)
        {
            try
            {
                return new LiteReader(Conecction(), sql);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al compilar y ejecutar un leector");
                return null;
            }
        }
        public LiteReader Leector(string sql, SqlConnection connection)
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
            using (IReader reader = Leector(sql))
            {
                return reader?.Read() ?? false;
            }
        }
        public bool TableExists(string TableName)
        {
            using (IReader reader = Leector($"SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}';"))
            {
                return reader?.Read() ?? false;
            }
        }
        public void Batch(string sql)
        {
            Batch(Conecction(), sql);
        }
        public void Batch(SQLiteConnection con, string sql)
        {
            StringBuilder sqlBatch = new StringBuilder();
            try
            {
                foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "--GO" || (sqlBatch.Length > 0 && sqlBatch[sqlBatch.Length - 1] == ';'))
                    {
                        string batch = sqlBatch.ToString();

                        if (!string.IsNullOrEmpty(batch))
                            EXEC(batch);
                        sqlBatch.Clear();
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

        public string FormatTime(TimeSpan TimeSpan)
        {
            return TimeSpan.ToString("hh':'mm':'ss'.'fff");
        }
        public string FormatTime(DateTime TimeSpan)
        {
            using (SQLiteConnection lite = Conecction())
            {
                return TimeSpan.ToString(lite.DateTimeStringFormat);
            }
        }

        public int LastScopeIdentity(SqlConnection con)
        {
           return Single<int>(con, "SELECT last_insert_rowid();");
        }
    }
}
