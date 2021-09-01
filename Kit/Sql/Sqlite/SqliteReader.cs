using System;
using System.Collections.Generic;
using System.Linq;
using Kit.Sql.Base;
using Kit.Sql.Helpers;
using Kit.Sql.Readers;
using Kit.Sql.SQLiteNetExtensions;

namespace Kit.Sql.Sqlite
{
    public class SqliteReader : IReader
    {
        private bool disposedValue;
        private List<ReaderItem> _Reader { get; set; }
        private CommandBase Command { get; set; }
        private SQLiteConnection Connection { get; set; }
        private string Query { get; set; }
        public int FieldCount { get => _Reader?.FirstOrDefault()?.Fields?.Count ?? 0; }
        public List<string> Fields => _Reader?.FirstOrDefault()?.Fields;
        public int Fila { get; private set; }

        internal SqliteReader(SQLiteConnection con, string Command, params object[] args)
        {
            this.Connection = con;
            this.Query = Command;
            this._Reader = null;
            this.Command = this.Connection.CreateCommand(this.Query, args);
        }

        public bool Read()
        {
            try
            {
                if (this.Connection.IsClosed)
                    this.Connection.RenewConnection();

                if (this._Reader is null)
                {
                    //var reader = this.Command.ExecuteReader();

                    this._Reader = this.Connection.ExecuteReader(Query).ToList();
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
                Log.Logger.Error(ex, "");
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
                        Log.Logger.Error(ex, "Desechando objetos");
                    }
                }
                this.Query = null;
                this.Connection = null;
                disposedValue = true;
            }
        }

        ~SqliteReader()
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

        public T Get<T>(int index) where T : IConvertible
        {
            return Sqlh.Parse<T>(this[index]);
        }
    }
}