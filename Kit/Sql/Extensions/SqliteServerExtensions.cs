using Kit.Sql.Helpers;
using Kit.Sql.Readers;
using Kit.Sql.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Kit
{
    public static class SqliteExtensions
    {

        public static List<T> Lista<T>(this SQLiteConnection con, string sql, int index = 0, params object[] parameters) where T : IConvertible
        {
            List<T> result = new List<T>();
            try
            {
                using (var cmd = con.CreateCommand(sql, parameters))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(Sqlh.Parse<T>(reader[index]));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.Logger.Error(ex, sql);
            }

            return result;
        }
        public static List<T> Lista<T>(this SQLiteConnection connection, string sql, params object[] parameters) where T : IConvertible
        {
            return connection.Lista<T>(sql, 0, parameters);
        }

        public static List<T> Lista<T>(this SQLiteConnection connection, string sql) where T : IConvertible
        {
            return connection.Lista<T>(sql: sql, index: 0, parameters: null);
        }

        public static object Single(this SQLiteConnection con, string sql, params object[] parameters)
        {
            object result = default;

            using (var cmd = con.CreateCommand(sql, parameters))
            {
                using (var reader = cmd.ExecuteReader())
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
            return result;
        }

        public static T Single<T>(this SQLiteConnection connection, string sql)
        {
            return connection.Single<T>(sql);
        }

        public static object Single(this SQLiteConnection connection, string sql)
        {
            return connection.Single(sql, null);
        }

        public static T Single<T>(this SQLiteConnection connection, string sql, params object[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(connection.Single(sql, null, parameters));
        }

        private const int Error = -2;

        public static int Execute(this SQLiteConnection connection, string sql, IEnumerable<object> parametros)
        {
            return connection.Execute(sql, parametros.ToArray());
        }
        public static int Execute(this SQLiteConnection con, string sql, params object[] parametros)
        {
            int Rows = Error;
            try
            {

                using (var cmd = con.CreateCommand(sql, parametros))
                {
                    Rows = cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Execute");
            }
            return Rows;
        }

        public static void Read(this SQLiteConnection connection, string sql, Action<IReader> OnRead, CommandConfig config = null, IEnumerable<object> parameters = null)
        {
            connection.Read(sql, OnRead, config, parameters?.ToArray());
        }

        public static void Read(this SQLiteConnection connection, string sql, Action<IReader> OnRead, IEnumerable<object> parameters)
        {
            connection.Read(sql, OnRead, parameters.ToArray());
        }

        public static void Read(this SQLiteConnection connection, string sql, Action<IReader> OnRead, params object[] parameters)
        {
            connection.Read(sql, OnRead, null, parameters);
        }

        public static void Read(this SQLiteConnection connection, string sql, Action<IReader> OnRead, CommandConfig config = null, params object[] parameters)
        {
            try
            {
                using (SQLiteConnection con = connection)
                {
                    using (var cmd = connection.CreateCommand(sql, parameters))
                    {
                        using (IReader reader = cmd.ExecuteReader())
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
    }
}
