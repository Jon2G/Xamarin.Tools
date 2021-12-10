using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Entity.Abstractions;

namespace Kit.Entity.Containers
{
    public class DatabaseContainer
    {
        private static DatabaseContainer _Current;
        public static DatabaseContainer Current => _Current ??= new DatabaseContainer();
        public readonly Dictionary<DatabaseTypes, DatabaseContext> DatabaseContexts;

        public DatabaseContext this[DatabaseTypes type]
        {
            get
            {
                if (DatabaseContexts.TryGetValue(type, out DatabaseContext context))
                {
                    return context;
                }
                LazyContextInfo lazy = LazyConnections[type];
                DatabaseContext dbcontext = lazy.BuildDatabaseContext();
                DatabaseContexts.Add(type, dbcontext);
                return dbcontext;
            }
        }
        public DatabaseContext this[IDbConnection connection]
        {
            get
            {
                var dbContext =DatabaseContexts.First(x => x.Value.DbConnection == connection).Value;
                return dbContext;
            }
        }

        private readonly Dictionary<DatabaseTypes, LazyContextInfo> LazyConnections;
        public DatabaseContainer()
        {
            DatabaseContexts = new Dictionary<DatabaseTypes, DatabaseContext>();
            LazyConnections = new();
        }

        public DatabaseContainer Register<T>(Func<Type[]> dbTypesFunc,Func<T> func, string DbName, uint version) where T : IDbConnection
        {
            DatabaseTypes dbType = Kit.Entity.ConnectionExtensions.GetDatabaseType<T>();
            LazyConnections.Add(dbType, new LazyContextInfo<T>(dbTypesFunc,func, DbName, version));
            return this;
        }
    }
}
