using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Kit.Entity.Abstractions;

namespace Kit.Entity.Containers
{
    public abstract class LazyContextInfo
    {
        protected readonly uint Version;
        protected readonly string DatabaseName;
        protected readonly Func<Type[]> DbTypes;
        public LazyContextInfo(string databaseName, uint version, Func<Type[]> dbTypes)
        {
            this.Version = version;
            this.DatabaseName = databaseName;
            this.DbTypes = dbTypes;
        }
        public abstract DatabaseContext BuildDatabaseContext();
    }
    public class LazyContextInfo<T> : LazyContextInfo where T : IDbConnection
    {
        private Func<T> Func;
        public LazyContextInfo(Func<Type[]> dbTypes, Func<T> func, string databaseName, uint version) : base(databaseName, version, dbTypes)
        {
            this.Func = func;
        }

        public override DatabaseContext BuildDatabaseContext()
        {
            T connection = this.Func.Invoke();
            var databaseContexts = new DatabaseContext(DatabaseName, connection,Version);
            return databaseContexts.Build(this.DbTypes.Invoke());
        }
    }
}
