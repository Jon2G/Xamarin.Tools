using System;
using System.Collections.Generic;
using System.Text;
//Alias para poder trabajar con mas facilidad
using SqlConnection = SQLite.SQLiteConnection;
using SqlCommand = SQLite.SQLiteCommand;
using SqlDataReader = SQLHelper.SQLiteNetExtensions.ReaderItem;
using System.Linq;
using System.Diagnostics;
using SQLHelper.SQLiteNetExtensions;
namespace SQLHelper.Readers
{
    public class LiteReader : IReader
    {
        private bool disposedValue;
        private List<SqlDataReader> _Reader { get; set; }
        private SqlConnection Connection { get; set; }
        private string Command { get; set; }
        public int FieldCount { get => _Reader?.FirstOrDefault()?.Fields?.Count ?? 0; }
        public List<string> Fields => _Reader?.FirstOrDefault()?.Fields;
        public int Fila { get; private set; }
        internal LiteReader(SqlConnection con, string Command)
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
            try
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
            catch (Exception ex)
            {
                Log.LogMe(ex);
                return false;
            }
            finally
            {
                Connection.Close();
            }
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
        public object this[string columna] => this._Reader[Fila][columna];

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
        ~LiteReader()
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
}
