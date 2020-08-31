using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQLHelper.Abstractions
{
    public class Insert : IQuery
    {

        private Insert(BaseSQLHelper SQLH, string TableName) : base(SQLH, TableName)
        {

        }
        public static Insert BulidFrom(BaseSQLHelper SQLH, string TableName)
        {
            return new Insert(SQLH, TableName);
        }

        public IQuery AddField(string Name, object Value)
        {
            this.Parameters.Add(Name, Value);
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
            if (this.Parameters.Any())
            {
                builder.Append(" WHERE");
                foreach (var pair in this.Parameters)
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
                IEnumerable<SqlParameter> parameters =
                    this.Parameters.Select(x => new SqlParameter(x.Key, x.Value));

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
