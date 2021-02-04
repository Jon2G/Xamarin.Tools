using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Sql.Interfaces
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
        /// <summary>
        /// Ejecuta una consulta sin parametros ni argumentos en la conexión actual
        /// </summary>
        /// <param name="sql"></param>
        public abstract int EXEC(string sql);
        /// <summary>
        /// Regresa el primer elemento leido en la primer columna y le realiza un casting a T
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract T Single<T>(string sql);
        /// <summary>
        /// Retorna un objeto IReader resultado de la consulta proporcionada
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract IReader Read(string sql);
        public void Dispose()
        {

        }

        protected T Parse<T>(object obj)
        {
            var type = typeof(T);
            try
            {

                if (type.IsEnum)
                {
                    return (T)Enum.Parse(typeof(T), obj.ToString(), true);
                }

                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, $"Al convertir un dato desde Parse<T> el tipo de dato: {type.Name}=>{obj}");
            }

            return default(T);
        }
    }
}
