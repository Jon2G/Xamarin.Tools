using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Base;

namespace Kit.Sql.Interfaces
{
    public abstract class IQuery: IDisposable
    {
        public readonly SqlBase SQLH;
        protected readonly Dictionary<string, object> Parameters;
        protected readonly string TableName;
        //protected bool PreventSqlInjection;
        protected IQuery(SqlBase SQLH, string TableName)
        {
            this.SQLH = SQLH;
            this.Parameters = new Dictionary<string, object>();
            this.TableName = TableName;
            //this.PreventSqlInjection = false;
        }
        public abstract int Execute();

        public IQuery AddParameter(string Name, object Value)
        {
            this.Parameters.Add(Name, Value);
            return this;
        }
        protected abstract string BuildQuery();
        protected abstract string BuildLiteQuery();
        public virtual void Dispose()
        {
            this.Parameters.Clear();
        }
        //public IQuery PreventInjection(bool PreventInjection = true)
        //{
        //    this.PreventSqlInjection = PreventInjection;
        //    return this;
        //}
    }
}
