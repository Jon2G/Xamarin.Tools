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
        private bool ReplaceOnSqlite;
        private readonly Dictionary<string, object> WhereParameters;
        private Update(BaseSQLHelper SQLH, string TableName) : base(SQLH, TableName)
        {
            this.ReplaceOnSqlite = true;
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
            StringBuilder builder = new StringBuilder();
            if (this.ReplaceOnSqlite)
            {
                builder
                    .Append("REPLACE INTO ")
                    .Append(TableName)
                    .Append(" (")
                    .Append(string.Join(",", this.Parameters.Select(x => x.Key)));
                if (this.WhereParameters.Any())
                {
                    builder.Append(',').Append(string.Join(",", this.WhereParameters.Select(x => x.Key)));
                }
                builder.Append(") VALUES (")
                    .Append(string.Join(",", this.Parameters.Select(x => "?")));
                if (this.WhereParameters.Any())
                {
                    builder.Append(',').Append(string.Join(",", this.WhereParameters.Select(x => "?")));
                }
                builder.Append(")");

            }
            else
            {
                builder
                    .Append("UPDATE ")
                    .Append(TableName)
                    .Append(" SET ");
                foreach (var field in this.Parameters)
                {
                    builder.Append(field.Key).Append("=?,");
                }
                if (builder[builder.Length - 1] == ',')
                {
                    builder = builder.Remove(builder.Length - 1, 1);
                }

                if (this.WhereParameters.Any())
                {
                    builder.Append(" WHERE ");
                }
                foreach (var field in this.Parameters)
                {
                    builder.Append(field.Key).Append("=? AND ");
                }
                string query = builder.ToString().Trim();

                if (query.EndsWith("AND"))
                {
                    int index = query.LastIndexOf("AND");
                    builder = builder.Remove(index, 3);
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
                List<object> parameters = this.Parameters.Select(x => x.Value).ToList();

                if (this.ReplaceOnSqlite)
                {
                    parameters.AddRange(this.WhereParameters.Select(x => x.Value));
                }
                return sqlite.EXEC(BuildLiteQuery(), parameters.ToArray());
            }
            throw new NotSupportedException("No sql connection set");
        }

        public Update NoReplaceOnSqlite()
        {
            this.ReplaceOnSqlite = false;
            return this;
        }
    }
}
