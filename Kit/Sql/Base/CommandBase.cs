using System;
using System.Collections.Generic;
using Kit.Daemon.Abstractions;
using Kit.Sql.Readers;

namespace Kit.Sql.Base
{
    public abstract class CommandBase : IDisposable
    {
        public abstract void Dispose();
        public abstract int ExecuteNonQuery();
        public abstract T ExecuteScalar<T>();
        public abstract IEnumerable<T> ExecuteQueryScalars<T>();
        public abstract List<T> ExecuteQuery<T>();
        public abstract IEnumerable<T> ExecuteDeferredQuery<T>();
        public abstract IEnumerable<T> ExecuteDeferredQuery<T>(TableMapping map);
        public abstract List<T> ExecuteQuery<T>(TableMapping map);
        public abstract IReader ExecuteReader();
        /// <summary>
        /// Invoked every time an instance is loaded from the database.
        /// </summary>
        /// <param name='obj'>
        /// The newly created object.
        /// </param>
        /// <remarks>
        /// This can be overridden in combination with the <see cref="SQLiteConnection.NewCommand"/>
        /// method to hook into the life-cycle of objects.
        /// </remarks>
        protected abstract void OnInstanceCreated(object obj);

    }
}
