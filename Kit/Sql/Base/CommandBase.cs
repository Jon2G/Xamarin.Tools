using System;
using System.Collections.Generic;
using System.Text;

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


    }
}
