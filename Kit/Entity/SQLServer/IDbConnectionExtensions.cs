using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Kit.Db.Abstractions;

namespace Kit.Entity
{
    public static class IDbConnectionExtensions
    {
        public static IDbCommand BuildCommand(this IDbConnection connection, string cmdText, CommandType commandType = CommandType.Text, params IDataParameter[] parameters)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.CommandType = commandType;
            if (parameters.Any())
            {
                foreach (IDataParameter dataParameter in parameters)
                {
                    cmd.Parameters.Add(dataParameter);
                }
            }
            return cmd;
        }

        public static IDbCommand BuildCommand(this IDbConnection connection, string cmdText, IDataParameter[] parameters)
            => connection.BuildCommand(cmdText, CommandType.Text, parameters);
    }
}
