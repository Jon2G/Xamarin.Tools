using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Kit.Db.Abstractions
{
    public class DbParameter<T> : DbParameter
    {
        public new T Value { get; set; }

        public DbParameter(string name, T value) : base(name, (object)value)
        {

        }
    }
    public class DbParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public DbParameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
    public static class DbParameterExtensions
    {
        public static SqlParameter ToSQLServer(this DbParameter dbParameter)
        {
            return new SqlParameter(dbParameter.Name, dbParameter.Value);
        }

    }
}
