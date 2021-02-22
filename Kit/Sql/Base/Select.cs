using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using Kit.Sql.Readers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kit.Sql.Abstractions
{
    public class Select : IQuery
    {
        private readonly List<string> SelectionFields;
        private Select(BaseSQLHelper SQLH, string TableName) : base(SQLH, TableName)
        {
            this.SelectionFields = new List<string>();
        }
        private Select(BaseSQLHelper SQLH) : base(SQLH, null)
        {
            this.SelectionFields = null;
        }
        public static Select BulidFrom(BaseSQLHelper SQLH)
        {
            return new Select(SQLH);
        }
        public IReader ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            if (this.SQLH is SqLite lite)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    Regex regex = new Regex(@$"\@{parameter.ParameterName}");
                    query = regex.Replace(query, $"'{parameter.Value}'");
                }
                return lite.Read(query);
            }
            else if (this.SQLH is SqlServer sql)
            {
                return sql.Read(query, CommandType.Text, false, parameters);
            }
            throw new NotSupportedException("No sql connection set");
        }
        public static Select BulidFrom(BaseSQLHelper SQLH, string TableName)
        {
            return new Select(SQLH, TableName);
        }
        public Select AddFields(params string[] Fields)
        {
            this.SelectionFields.AddRange(Fields);
            return this;
        }
        public Select Where(string Field, object value)
        {
            //if (this.PreventSqlInjection && value is string s && s.Contains('\''))
            //{
            //    Log.DebugMe("[WARNING] INYECCIÓN DE SQL EVITADA");
            //    return (Select)this.AddParameter(Field, s.Replace("\'", ""));
            //}
            return (Select)this.AddParameter(Field, value);
        }
        protected override string BuildQuery()
        {
            StringBuilder builder = new StringBuilder()
                .Append("SELECT ")
                  .Append(string.Join(",", this.SelectionFields))
                  .Append(" FROM ")
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
            StringBuilder builder = new StringBuilder().Append("SELECT ");
            if (!this.SelectionFields.Any())
            {
                builder.Append(" * ");
            }
            else
            {
                builder.Append(string.Join(",", this.SelectionFields));
            }
            builder.Append(" FROM ").Append(TableName);
            if (this.Parameters.Any())
            {
                builder.Append(" WHERE");
                foreach (KeyValuePair<string, object> pair in this.Parameters)
                {
                    builder.Append(" ")
                        .Append(pair.Key)
                        .Append(" ='")
                        .Append(pair.Value)
                        .Append('\'');//.Append(" AND");
                }
                //if (this.Parameters.Any())
                //{
                //    builder.Remove(builder.Length - 3, 3);
                //}
            }
            return builder.ToString();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.SelectionFields.Clear();
            return;
        }

        public override int Execute()
        {
            return ExecuteReader().Read() ? 1 : 0;
        }
        public IReader ExecuteReader()
        {
            if (this.SQLH is SqlServer sql)
            {
                IEnumerable<SqlParameter> parameters =
                    this.Parameters.Select(x => new SqlParameter(x.Key, x.Value)).ToArray();

                return sql.Read(BuildQuery(), System.Data.CommandType.Text, false, parameters.ToArray());
            }
            else if (this.SQLH is SqLite sqlite)
            {
                return sqlite.Read(BuildLiteQuery());
            }
            throw new NotSupportedException("No sql connection set");
        }
        //public new Select PreventInjection(bool PreventInjection = true)
        //{
        //    return (Select)base.PreventInjection(PreventInjection);
        //}

    }
}
