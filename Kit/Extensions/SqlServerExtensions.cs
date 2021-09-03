using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Kit.Sql.Helpers;

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
            return connection.Single(sql, CommandType.Text, parameters);
        }

        public static object Single(this SqlConnection connection, string sql, CommandType type, params SqlParameter[] parameters)
        {
            object result = default;
            using (SqlConnection con = connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, connection) { CommandType = type })
                {
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
            return Sqlh.Parse<T>(connection.Single(sql, type, parameters));
        }

        public static T Single<T>(this SqlConnection connection, string sql, params SqlParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(connection.Single(sql, parameters));
        }
        private const int Error = -2;

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
        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead)
        {
            connection.Read(sql, OnRead, CommandType.Text);
        }

        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead, params SqlParameter[] parameters)
        {
            connection.Read(sql, OnRead, CommandType.Text, parameters);
        }

        public static void Read(this SqlConnection connection, string sql, Action<SqlDataReader> OnRead, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection con = connection)
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, connection) { CommandType = commandType })
                    {
                        if (parameters is not null)
                            cmd.Parameters.AddRange(parameters);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OnRead.Invoke(reader);
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Transaccion fallida reportada");
                Log.Logger.Error(sql);
            }
        }
    }
}
