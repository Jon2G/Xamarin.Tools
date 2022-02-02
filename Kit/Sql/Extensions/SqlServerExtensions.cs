using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kit.Enums;
using Kit.Sql.Helpers;
using Kit.Sql.Readers;
using Kit.Sql.SqlServer;

namespace Kit
{
    public static class SqlServerExtensions
    {
        public static List<T> Lista<T>(this SqlConnection connection, string sql, CommandType type = CommandType.Text, int index = 0,
            params SqlParameter[] parameters) where T : IConvertible
        {
            List<T> result = new List<T>();
            try
            {
                using (SqlConnection con = connection)
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
                    {
                        cmd.Parameters.AddRange(parameters);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(Sqlh.Parse<T>(reader[index]));
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.Logger.Error(ex, sql);
            }

            return result;
        }

        public static List<T> Lista<T>(this SqlConnection connection, string sql, int indice = 0, params SqlParameter[] parameters) where T : IConvertible
        {
            return connection.Lista<T>(sql, CommandType.Text, indice, parameters);
        }

        public static List<T> Lista<T>(this SqlConnection connection, string sql, params SqlParameter[] parameters) where T : IConvertible
        {
            return connection.Lista<T>(sql, CommandType.Text, 0, parameters);
        }

        public static List<T> Lista<T>(this SqlConnection connection, string sql) where T : IConvertible
        {
            return connection.Lista<T>(sql: sql, type: CommandType.Text, index: 0, parameters: null);
        }

        public static object Single(this SqlConnection connection, string sql, params SqlParameter[] parameters)
        {
            return connection.Single(sql, CommandType.Text, null, parameters);
        }

        public static object Single(this SqlConnection connection, string sql, CommandType type, int? timeOut = null, params SqlParameter[] parameters)
        {
            object result = default;
            using (SqlConnection con = connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, connection) { CommandType = type })
                {
                    if (timeOut is not null)
                    {
                        cmd.CommandTimeout = (int)timeOut;
                    }
                    if (parameters is not null)
                        cmd.Parameters.AddRange(parameters);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader[0] != DBNull.Value)
                            {
                                result = (reader[0]);
                            }
                        }
                    }
                }
                con.Close();
            }
            return result;
        }

        public static T Single<T>(this SqlConnection connection, string sql)
        {
            return connection.Single<T>(sql);
        }

        public static object Single(this SqlConnection connection, string sql)
        {
            return connection.Single(sql, null);
        }

        public static T Single<T>(this SqlConnection connection, string sql, CommandType type, params SqlParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(connection.Single(sql, type, null, parameters));
        }

        public static T Single<T>(this SqlConnection connection, string sql, params SqlParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(connection.Single(sql, parameters));
        }

        private const int Error = -2;

        public static int Execute(this SqlConnection connection, string sql, IEnumerable<SqlParameter> parametros)
        {
            return connection.Execute(sql, parametros?.ToArray());
        }

        public static int Execute(this SqlConnection connection, string sql, params SqlParameter[] parametros)
        {
            return connection.Execute(sql, CommandType.Text, parametros);
        }

        public static int Execute(this SqlConnection connection, string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parametros)
        {
            int Rows = Error;
            try
            {
                using (var con = connection)
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = commandType })
                    {
                        if (parametros is not null)
                        {
                            if (parametros.Any(x => x.Value is null))
                            {
                                foreach (SqlParameter t in parametros)
                                {
                                    if (t.Value is null)
                                    {
                                        t.Value = DBNull.Value;
                                    }

                                    if (!parametros.Any(x => x.Value is null))
                                        break;
                                }
                            }
                            cmd.Parameters.AddRange(parametros);
                        }
                        Rows = cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Execute");
            }
            return Rows;
        }

        public static void Read(this SQLServerConnection connection, string sql, Action<IReader> OnRead)
        {
            var con = connection.Con();
            con.Open();
            using (var cmd = new SqlCommand(sql, con))
            {
                using (var reader = new Kit.Sql.Readers.Reader(cmd, connection))
                {
                    while (reader.Read())
                    {
                        OnRead.Invoke(reader);
                    }
                }

            }
        }
        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead, CommandConfig config = null, IEnumerable<SqlParameter> parameters = null)
        {
            connection.Read(sql, OnRead, config, parameters?.ToArray());
        }

        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead, IEnumerable<SqlParameter> parameters)
        {
            connection.Read(sql, OnRead, parameters.ToArray());
        }

        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead, params SqlParameter[] parameters)
        {
            connection.Read(sql, OnRead, null, parameters);
        }

        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead, CommandConfig config = null, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection con = connection)
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        if (config is not null)
                        {
                            cmd.CommandType = config.CommandType;
                            cmd.CommandTimeout = config.CommandTimeout;
                        }
                        if (parameters is not null)
                            cmd.Parameters.AddRange(parameters);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (config?.ManualRead ?? false)
                            {
                                OnRead.Invoke(reader);
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    OnRead.Invoke(reader);
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.IsDBConnectionError(ex);
                Log.Logger.Error(ex, "Transaccion fallida reportada");
                Log.Logger.Error(sql);
            }
        }
        public static DataTable DataTable(this SqlConnection connection, string sql, string TableName = null)
        {
            return connection.DataTable(sql, TableName, new CommandConfig() { CommandType = CommandType.Text });
        }

        public static DataTable DataTable(this SqlConnection connection, string sql, string TableName = null, params SqlParameter[] parameters)
        {
            return connection.DataTable(sql, TableName, new CommandConfig() { CommandType = CommandType.Text }, parameters);
        }
        public static DataTable DataTable(this SqlConnection connection, string sql, string TableName = null, CommandConfig config = null, params SqlParameter[] parameters)
        {
            config = config ?? new CommandConfig();
            DataTable result = new DataTable(TableName);
            using (SqlConnection con = connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = config.CommandType })
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    try
                    {
                        result.Load(cmd.ExecuteReader());
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "");
                        throw;
                    }
                }
                con.Close();
            }
            return result;
        }

        public static SqlServerInformation GetServerInformation(this SqlConnection connection)
        {
            Regex regex = new Regex(@"(?<Version>\d+).*");
            SqlServerInformation inf = new SqlServerInformation(Enums.SqlServerVersion.None, string.Empty, string.Empty);
            connection.Read("SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY('productlevel'), SERVERPROPERTY('edition')",
            (reader) =>
            {
                int ver = 0;
                string version = reader[0].ToString();
                var match = regex.Match(version);
                if (match.Success)
                {
                    version = match.Groups["Version"].Value;
                    ver = Convert.ToInt32(version);
                }
                inf = new SqlServerInformation((SqlServerVersion)ver, reader[1].ToString(), reader[2].ToString());
            });
            return inf;
        }
    }
}