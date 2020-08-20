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

namespace SQLHelper
{
    public class SQLHLite
    {
        public class Reader : IDisposable
        {
            private bool disposedValue;
            private List<SqlDataReader> _Reader { get; set; }
            private SqlConnection Connection { get; set; }
            private string Command { get; set; }
            public int FieldCount { get => _Reader?.FirstOrDefault()?.Fields?.Count ?? 0; }
            public List<string> Fields => _Reader?.FirstOrDefault()?.Fields;
            public int Fila { get; private set; }
            internal Reader(SqlConnection con, string Command)
            {
                this.Connection = con;
                this.Command = Command;
                this._Reader = null;
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(Command);
                }
            }

            public bool Read()
            {
                if (this._Reader is null)
                {
                    this._Reader = this.Connection.ExecuteReader(Command).ToList();
                    this.Fila = 0;
                }
                else
                {
                    Fila++;
                }

                return (this._Reader.Count > this.Fila);
            }
            public object this[int index]
            {
                get
                {
                    ReaderItem fila = this._Reader[Fila];
                    string campo = fila.Fields[index];
                    return fila[campo];
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        try
                        {
                            Connection?.Close();
                            Connection?.Dispose();

                        }
                        catch (Exception ex)
                        {
                            Log.LogMe(ex, "Desechando objetos");
                        }
                    }
                    this.Command = null;
                    this.Connection = null;
                    disposedValue = true;
                }
            }
            ~Reader()
            {
                // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

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
        public async Task RevisarBaseDatos()
        {
            await Task.Yield();
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
                using (Reader reader = new Reader(Conecction(), "SELECT VERSION FROM DB_VERSION"))
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
                using (Reader reader = Leector(sql))
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
            int afectadas = -1;
            using (SqlConnection con = Conecction())
            {
                afectadas = con.Execute(sql, parametros);
                con.Close();
            }
            if (Debugger.IsAttached)
            {
                Console.WriteLine(sql);
            }
            return afectadas;
        }
        public List<T> Lista<T>(string sql)
        {
            List<T> result = new List<T>();
            using (Reader reader = Leector(sql))
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

            using (Reader reader = Leector(sql))
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
            using (Reader reader = Leector(Querry))
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
        public Reader Leector(string sql)
        {
            try
            {
                return new Reader(Conecction(), sql);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al compilar y ejecutar un leector");
                return null;
            }
        }
        public Reader Leector(string sql, SqlConnection connection)
        {
            try
            {
                return new Reader(connection, sql);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al compilar y ejecutar un leector");
                return null;
            }
        }
        public bool Exists(string sql)
        {
            using (Reader reader = Leector(sql))
            {
                return reader?.Read() ?? false;
            }
        }
        internal void Batch(string sql)
        {
            StringBuilder sqlBatch = new StringBuilder();
            sql += "\nGO";   // make sure last batch is executed.
            try
            {
                foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        if (!string.IsNullOrEmpty(sqlBatch.ToString()))
                            EXEC(sqlBatch.ToString());
                        sqlBatch.Clear();
                    }
                    else
                    {
                        sqlBatch.AppendLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex.Message);
            }
        }


    }
}
