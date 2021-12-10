using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kit.Db.Abstractions;
using Kit.Sql.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Kit.Entity
{
    public static class ReaderExtensions
    {
        public static List<T> Lista<T>(this IDbConnection connection, string sql, CommandType type = CommandType.Text, int index = 0,
            params IDataParameter[] parameters) where T : IConvertible
        {
            List<T> result = new List<T>();
            try
            {
                using (IDbConnection con = connection)
                {
                    con.Open();
                    using (IDbCommand cmd = con.BuildCommand(sql, type, parameters))
                    {
                        using (IDataReader reader = cmd.ExecuteReader())
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

        public static List<T> Lista<T>(this IDbConnection connection, string sql, int indice = 0, params IDataParameter[] parameters) where T : IConvertible
        {
            return connection.Lista<T>(sql, CommandType.Text, indice, parameters);
        }

        public static List<T> Lista<T>(this IDbConnection connection, string sql, params IDataParameter[] parameters) where T : IConvertible
        {
            return connection.Lista<T>(sql, CommandType.Text, 0, parameters);
        }

        public static List<T> Lista<T>(this IDbConnection connection, string sql) where T : IConvertible
        {
            return connection.Lista<T>(sql: sql, type: CommandType.Text, index: 0, parameters: null);
        }

        public static object Single(this IDbConnection connection, string sql, params IDataParameter[] parameters)
        {
            return connection.Single(sql, CommandType.Text, parameters);
        }

        public static object Single(this IDbConnection connection, string sql, CommandType type, params IDataParameter[] parameters)
        {
            object result = default;
            using (IDbConnection con = connection)
            {
                con.Open();
                using (IDbCommand cmd = connection.BuildCommand(sql, type, parameters))
                {
                    using (IDataReader reader = cmd.ExecuteReader())
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

        public static T Single<T>(this IDbConnection connection, string sql)
        {
            return connection.Single<T>(sql);
        }

        public static object Single(this IDbConnection connection, string sql)
        {
            return connection.Single(sql, null);
        }

        public static T Single<T>(this IDbConnection connection, string sql, CommandType type, params IDataParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(connection.Single(sql, type, parameters));
        }

        public static T Single<T>(this IDbConnection connection, string sql, params IDataParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(connection.Single(sql, parameters));
        }

        private const int Error = -2;

        public static int Execute(this IDbConnection connection, string sql, IEnumerable<IDataParameter> parametros)
        {
            return connection.Execute(sql, parametros.ToArray());
        }

        public static int Execute(this IDbConnection connection, string sql, params IDataParameter[] parametros)
        {
            return connection.Execute(sql, CommandType.Text, parametros);
        }

        public static int Execute(this IDbConnection connection, string sql, CommandType commandType = CommandType.Text, params IDataParameter[] parameters)
        {
            int Rows = Error;
            try
            {
                using (var con = connection)
                {
                    con.Open();
                    using (IDbCommand cmd = connection.BuildCommand(sql, commandType, parameters))
                    {
                        if (parameters.Any(x => x.Value is null))
                        {
                            foreach (IDataParameter t in parameters)
                            {
                                if (t.Value is null)
                                {
                                    t.Value = DBNull.Value;
                                }

                                if (!parameters.Any(x => x.Value is null))
                                    break;
                            }
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

        public static void Read(this IDbConnection connection, string sql, Action<IDataReader> OnRead, CommandConfig config = null, IEnumerable<IDataParameter> parameters = null)
        {
            connection.Read(sql, OnRead, config, parameters?.ToArray());
        }

        public static void Read(this IDbConnection connection, string sql, Action<IDataReader> OnRead, IEnumerable<IDataParameter> parameters)
        {
            connection.Read(sql, OnRead, parameters.ToArray());
        }

        public static void Read(this IDbConnection connection, string sql, Action<IDataReader> OnRead, params IDataParameter[] parameters)
        {
            connection.Read(sql, OnRead, null, parameters);
        }

        public static void Read(this IDbConnection connection, string sql, Action<IDataReader> OnRead, CommandConfig config = null, params IDataParameter[] parameters)
        {
            try
            {
                using (IDbConnection con = connection)
                {
                    con.Open();
                    using (IDbCommand cmd = connection.BuildCommand(sql, config?.CommandType ?? CommandType.Text, parameters))
                    {
                        if (config is not null)
                        {
                            cmd.CommandType = config.CommandType;
                            cmd.CommandTimeout = config.CommandTimeout;
                        }
                        using (IDataReader reader = cmd.ExecuteReader())
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
                Log.Logger.Error(ex, "Transaccion fallida reportada");
                Log.Logger.Error(sql);
            }
        }
        public static DataTable DataTable(this IDbConnection connection, string sql, string TableName = null)
        {
            return connection.DataTable(sql, TableName, new CommandConfig() { CommandType = CommandType.Text });
        }
        public static DataTable DataTable(this IDbConnection connection, string sql, string TableName = null, CommandConfig config = null, params IDataParameter[] parameters)
        {
            config = config ?? new CommandConfig();
            DataTable result = new DataTable(TableName);
            using (IDbConnection con = connection)
            {
                con.Open();
                using (IDbCommand cmd = con.BuildCommand(sql, config?.CommandType ?? CommandType.Text, parameters))
                {
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
    }
}
