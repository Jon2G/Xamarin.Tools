using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Kit.Sql.Enums;
using Kit.Sql.Readers;
using Kit.Sql.Sqlite;

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

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// If a UNIQUE constraint violation occurs with
        /// some pre-existing object, this function deletes
        /// the old object.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows modified.
        /// </returns>
        public int InsertOrReplace(object obj, Type objType)
        {
            return Insert(obj, "OR REPLACE", objType);
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// If a UNIQUE constraint violation occurs with
        /// some pre-existing object, this function deletes
        /// the old object.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <returns>
        /// The number of rows modified.
        /// </returns>
        public abstract int InsertOrReplace(object obj, bool shouldnotify = true);

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public int Insert(object obj, Type objType)
        {
            return Insert(obj, "", objType);
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="extra">
        /// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>

        public abstract int Insert(object obj, string extra);

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="extra">
        /// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public abstract int Insert(object obj, string extra, Type objType, bool shouldnotify = false);

        /// <summary>
        /// Deletes the given object from the database using its primary key.
        /// </summary>
        /// <param name="objectToDelete">
        /// The object to delete. It must have a primary key designated using the PrimaryKeyAttribute.
        /// </param>
        /// <returns>
        /// The number of rows deleted.
        /// </returns>
        public abstract int Delete(object objectToDelete);

        protected SqlBase()
        {
        }

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
            var key = GetTableMappingKey(TableMapping.GetTableName(type));
            lock (_mappings)
            {
                if (_mappings.TryGetValue(key, out map))
                {
                    if (map.MappedType != type)
                    {
                        map = _GetMapping(type, createFlags);
                        return map;
                    }
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

        //public string GetTableMappingKey(Type type)
        //{
        //    return type.FullName + MappingSuffix;
        //}
        public string GetTableMappingKey(string TableName)
        {
            StringBuilder sb = new StringBuilder(TableName).Append(MappingSuffix);
            return sb.ToString();
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

        public abstract BaseTableQuery Table(Type Type);

        public abstract SqlBase RenewConnection();

        /// <summary>
        /// /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// It returns each row of the result using the mapping automatically generated for
        /// the given type.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// An enumerable with one result for each row returned by the query.
        /// The enumerator (retrieved by calling GetEnumerator() on the result of this method)
        /// will call sqlite3_step on each call to MoveNext, so the database
        /// connection must remain open for the lifetime of the enumerator.
        /// </returns>
        public abstract IEnumerable<T> DeferredQuery<T>(string query, params object[] args) where T : new();

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

        public CreateTableResult CreateTable(Type ty, CreateFlags createFlags = CreateFlags.None)
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

        public abstract int Update(object obj);

        /// <summary>
        /// Updates all of the columns of a table using the specified object
        /// except for its primary key.
        /// The object is required to have a primary key.
        /// </summary>
        /// <param name="obj">
        /// The object to update. It must have a primary key designated using the PrimaryKeyAttribute.
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public abstract int Update(object obj, Type objType, bool shouldnotify = true);

        public abstract void Dispose();

        public abstract int Update<T>(T obj, Expression<Func<T, bool>> predicate, bool shouldnotify = true)
            where T : new();
    }
}