using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Kit.Sql.Helpers;

namespace Kit.Sql.Abstractions
{
    public class Delete : IQuery
    {
        private Delete(BaseSQLHelper SQLH, string TableName) : base(SQLH, TableName)
        {

        }
        public static Delete BulidFrom(BaseSQLHelper SQLH, string TableName)
        {
            return new Delete(SQLH, TableName);
        }

        public Delete Where(string Field, object value)
        {
            return (Delete)this.AddParameter(Field, value);
        }
        protected override string BuildQuery()
        {
            StringBuilder builder = new StringBuilder()
                .Append("DELETE FROM ")
                  .Append(TableName);
            if (this.Parameters.Any())
            {
                builder.Append(" WHERE");
                foreach (KeyValuePair<string, object> pair in this.Parameters)
                {
                    builder.Append(" ")
                        .Append(pair.Key)
                        .Append(" =@")
                        .Append(pair.Key);
                }
            }
            return builder.ToString();
        }
        protected override string BuildLiteQuery()
        {
            StringBuilder builder = new StringBuilder()
                .Append("DELETE FROM ")
                  .Append(TableName);
            if (this.Parameters.Any())
            {
                builder.Append(" WHERE");
                foreach (KeyValuePair<string, object> pair in this.Parameters)
                {
                    builder.Append(" ")
                        .Append(pair.Key)
                        .Append(" =?");
                }
            }
            return builder.ToString();
        }

        public override void Dispose()
        {
            base.Dispose();
            return;
        }

        public override int Execute()
        {
            if (this.SQLH is SqlServer sql)
            {
                IEnumerable<SqlParameter> parameters =
                    this.Parameters.Select(x => new SqlParameter(x.Key, x.Value)).ToArray();
                return sql.EXEC(BuildQuery(), System.Data.CommandType.Text, false, parameters.ToArray());
            }
            else if (this.SQLH is SqLite sqlite)
            {
                IEnumerable<object> parameters = this.Parameters.Select(x => x.Value);
                return sqlite.EXEC(BuildLiteQuery(),parameters.ToArray());
            }
            throw new NotSupportedException("No sql connection set");
        }

    }
}
