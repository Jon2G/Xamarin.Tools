using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQLHelper.Abstractions
{
    public class Update : IQuery
    {
        private readonly Dictionary<string, object> WhereParameters;
        private Update(BaseSQLHelper SQLH, string TableName) : base(SQLH, TableName)
        {
            this.WhereParameters = new Dictionary<string, object>();
        }
        public static Update BulidFrom(BaseSQLHelper SQLH, string TableName)
        {
            return new Update(SQLH, TableName);
        }

        public Update AddField(string Field, object NewValue)
        {
            this.AddParameter(Field, NewValue);
            return this;
        }
        public Update Where(string Field, object value)
        {
            this.WhereParameters.Add(Field, value);
            return this;
        }

        protected override string BuildQuery()
        {
            StringBuilder builder = new StringBuilder()
                .Append("UPDATE ")
                .Append(TableName)
                .Append(" SET ");
            foreach (var pair in this.Parameters)
            {
                builder
                    .Append(pair.Key)
                    .Append("=@")
                    .Append(pair.Key)
                    .Append(", ");
            }

            if (builder[builder.Length - 2] == ',')
            {
                builder.Remove(builder.Length - 2, 1);
            }
            if (this.WhereParameters.Any())
            {
                builder.Append(" WHERE");
                foreach (var pair in this.WhereParameters)
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
                .Append("REPLACE INTO ")
                .Append(TableName)
                .Append(" (")
                .Append(string.Join(",", this.Parameters.Select(x => x.Key)))
                .Append(") VALUES (")
                .Append(string.Join(",", this.Parameters.Select(x => "?")))
                .Append(")");
            return builder.ToString();
        }
        public override void Dispose()
        {
            return;
        }

        public override int Execute()
        {
            if (this.SQLH is SQLH sql)
            {
                List<SqlParameter> parameters =
                    this.Parameters.Select(x => new SqlParameter(x.Key, x.Value)).ToList();
                foreach (var where in this.WhereParameters)
                {
                    if (!parameters.Any(x => x.ParameterName == where.Key))
                    {
                        parameters.Add(new SqlParameter(where.Key, where.Value));
                    }
                }
                return sql.EXEC(BuildQuery(), System.Data.CommandType.Text, false, parameters.ToArray());
            }
            else if (this.SQLH is SQLHLite sqlite)
            {
                IEnumerable<object> parameters = this.Parameters.Select(x => x.Value);

                return sqlite.EXEC(BuildLiteQuery(), parameters.ToArray());
            }
            throw new NotSupportedException("No sql connection set");
        }
    }
}
