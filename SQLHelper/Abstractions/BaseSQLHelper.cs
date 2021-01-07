using System;
using System.Collections.Generic;
using System.Text;

namespace SQLHelper.Interfaces
{
    public abstract class BaseSQLHelper : IDisposable
    {
        public Exception LastException { get; protected set; }
        public event EventHandler OnConnectionStringChanged;
        public const int Error = -2;
        private string _ConnectionString;
        public string ConnectionString
        {
            get => _ConnectionString;
            protected set
            {
                if (_ConnectionString != value)
                {
                    _ConnectionString = value;
                    this.OnConnectionStringChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        protected BaseSQLHelper() { }
        public BaseSQLHelper(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }
        /// <summary>
        /// Deterima si existe una tabla con el nombre proporcionado
        /// </summary>
        /// <param name="TableName">Nombre de la tabla a buscar</param>
        /// <returns>Un booleano que indica si la table existe ó no</returns>
        public abstract bool TableExists(string TableName);
        public static string FormatTime(TimeSpan TimeSpan)
        {
            return $"{TimeSpan:hh}:{TimeSpan:mm}:{TimeSpan:ss}";
        }
        public static string FormatTime(DateTime TimeSpan)
        {
            //using (SQLiteConnection lite = Conecction())
            //{
            //'2020-09-17T12:27:55'  Formato universal de fecha y hora sql server
            return TimeSpan.ToString("yyyy-MM-ddTHH:mm:ss");
            //}
        }

        public void Dispose()
        {

        }
    }
}
