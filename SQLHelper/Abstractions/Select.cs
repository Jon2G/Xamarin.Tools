using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLHelper.Abstractions
{
    public class Select : IQuery
    {
        private readonly StringBuilder SelectionQuery;
        private Select(BaseSQLHelper SQLH, string SelectionQuery,string TableName) : base(SQLH, TableName)
        {
            this.SelectionQuery = new StringBuilder(SelectionQuery);
            if (this.SelectionQuery.Length == 0)
            {
                this.SelectionQuery.Append("SELECT ");
            }
        }
        public static Select BulidFrom(BaseSQLHelper SQLH, string Query, string TableName)
        {
            return new Select(SQLH, Query,TableName);
        }
        public static Select BulidFrom(BaseSQLHelper SQLH,string TableName)
        {
            return new Select(SQLH, string.Empty,TableName);
        }
        public Select AddFields(params string[] Fields)
        {
            this.SelectionQuery.Append(string.Join(",", Fields));
            return this;
        }
        public Select Where(string Field, object value)
        {
            return (Select)this.AddParameter(Field, value);
        }
        protected override string BuildQuery()
        {
            StringBuilder builder = new StringBuilder()
                  .Append(this.SelectionQuery)
                  .Append(" FROM ")
                  .Append(TableName);
            if (this.Parameters.Any())
            {
                builder.Append(" WHERE");
                foreach (var pair in this.Parameters)
                {
                    builder.Append(" ")
                        .Append(pair.Key)
                        .Append(" = ")
                        .Append(pair.Value);
                }
            }
            return builder.ToString();
        }

        public override void Dispose()
        {
            return;
        }

        public override int Execute()
        {
            return ExecuteReader().Read() ? 1 : 0;
        }
        public IReader ExecuteReader()
        {
            if (this.SQLH is SQLH sql)
            {
                return sql.Leector(BuildQuery(), System.Data.CommandType.Text, false);
            }
            else if (this.SQLH is SQLHLite sqlite)
            {
                return sqlite.Leector(BuildQuery());
            }
            throw new NotSupportedException("No sql connection set");

        }
    }
}
