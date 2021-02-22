using System;
using System.Collections.Generic;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using SQLServer;

namespace Kit.Sql.Base
{
    public abstract class SqlBase : IDisposable
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

        /// <summary>
        /// Whether <see cref="SQLiteConnection"/> has been disposed and the database is closed.
        /// </summary>
        public abstract bool IsClosed { get; }
        public abstract string MappingSuffix { get; }
        protected SqlBase() { }
        public SqlBase(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }
        protected readonly static Dictionary<string, TableMapping> _mappings = new Dictionary<string, TableMapping>();
        /// <summary>
        /// Returns the mappings from types to tables that the connection
        /// currently understands.
        /// </summary>
        public IEnumerable<TableMapping> TableMappings
        {
            get
            {
                lock (_mappings)
                {
                    return new List<TableMapping>(_mappings.Values);
                }
            }
        }

        protected abstract TableMapping _GetMapping(Type type, CreateFlags createFlags = CreateFlags.None);
        /// <summary>
        /// Retrieves the mapping that is automatically generated for the given type.
        /// </summary>
        /// <param name="type">
        /// The type whose mapping to the database is returned.
        /// </param>
        /// <param name="createFlags">
        /// Optional flags allowing implicit PK and indexes based on naming conventions
        /// </param>
        /// <returns>
        /// The mapping represents the schema of the columns of the database and contains
        /// methods to set and get properties of objects.
        /// </returns>
        public TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
        {
            TableMapping map;
            var key = GetTableMappingKey(type);
            lock (_mappings)
            {
                if (_mappings.TryGetValue(key, out map))
                {
                    if (createFlags != CreateFlags.None && createFlags != map.CreateFlags)
                    {
                        map = _GetMapping(type, createFlags);
                        _mappings[key] = map;
                    }
                }
                else
                {
                    map = _GetMapping(type, createFlags);
                    _mappings.Add(key, map);
                }
            }
            return map;
        }

        public string GetTableMappingKey(Type type)
        {
            return type.FullName + MappingSuffix;
        }

        /// <summary>
        /// Retrieves the mapping that is automatically generated for the given type.
        /// </summary>
        /// <param name="createFlags">
        /// Optional flags allowing implicit PK and indexes based on naming conventions
        /// </param>
        /// <returns>
        /// The mapping represents the schema of the columns of the database and contains
        /// methods to set and get properties of objects.
        /// </returns>
        public TableMapping GetMapping<T>(CreateFlags createFlags = CreateFlags.None)
        {
            return GetMapping(typeof(T), createFlags);
        }
        public abstract TableQuery<T> Table<T>() where T : new();
        public abstract void RenewConnection();
        public abstract CommandBase CreateCommand(string cmdText, params object[] ps);
        public abstract T Find<T>(object pk) where T : new();
        /// <summary>
        /// Executes a "create table if not exists" on the database. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated.
        /// </returns>
        public CreateTableResult CreateTable<T>(CreateFlags createFlags = CreateFlags.None)
        {
            return CreateTable(typeof(T), createFlags);
        }

        public abstract CreateTableResult CreateTable(TableMapping table, CreateFlags createFlags = CreateFlags.None);

        public  CreateTableResult CreateTable(Type ty, CreateFlags createFlags = CreateFlags.None)
        {
            return CreateTable(GetMapping(ty, createFlags), createFlags);
        }
        /// <summary>
        /// Deterima si existe una tabla con el nombre proporcionado
        /// </summary>
        /// <param name="TableName">Nombre de la tabla a buscar</param>
        /// <returns>Un booleano que indica si la table existe ó no</returns>
        public abstract bool TableExists(string TableName);

        public bool TableExists<T>() where T : new()
        {
            return TableExists(typeof(T));
        }
        public bool TableExists(Type type)
        {
            return TableExists(GetMapping(type).TableName);
        }
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
        public abstract T Single<T>(string sql) where T : IConvertible;
        public abstract object Single(string sql);
        /// <summary>
        /// Retorna un objeto IReader resultado de la consulta proporcionada
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract IReader Read(string sql);

        public abstract void Dispose();
    }
}
