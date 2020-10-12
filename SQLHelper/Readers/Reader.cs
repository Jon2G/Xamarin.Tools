using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SQLHelper.Readers
{
    public class Reader : IReader
    {
        private DbDataReader _Reader { get; set; }
        private SqlCommand Cmd { get; set; }
        private SqlConnection Connection { get; set; }
        public int FieldCount { get => _Reader.FieldCount; }
        public Reader(SqlCommand Cmd)
        {
            try
            {
                Connection = Cmd.Connection;
                this.Cmd = Cmd;
                _Reader = Cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.LogMe(ex);
                _Reader = new FakeReader();
            }
        }
        internal Reader() { }
        internal async Task<Reader> AsyncReader(SqlCommand Cmd)
        {
            try
            {
                Connection = Cmd.Connection;
                this.Cmd = Cmd;
                this.Cmd.CommandTimeout = 0;
                _Reader = await this.Cmd.ExecuteReaderAsync();
            }
            catch (Exception)
            {
                Log.LogMeSQL(this.Cmd.CommandText);
            }
            finally
            {
                Connection.Close();
            }
            return this;
        }
        public void Dispose()
        {
            try
            {
                if (_Reader?.IsClosed ?? false)
                    _Reader?.Close();
                Cmd?.Dispose();
                Connection?.Close();
                Connection?.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Desechando objetos");
            }
        }

        public bool Read()
        {
            bool read = _Reader.Read();
            if (!read)
            {
                Dispose();
            }
            return read;
        }

        public async Task<bool> Read(bool async)
        {
            return async ? await _Reader?.ReadAsync() : Read();
        }
        public object this[int index] => _Reader[index];
        public object this[string columna] => _Reader[columna];
    }
}
