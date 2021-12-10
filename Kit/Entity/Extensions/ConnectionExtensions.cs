using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Db.Abstractions;
using Kit.Entity.Abstractions;
using Kit.Entity.Containers;
using Kit.Sql.Interfaces;
using Kit.Sql.Tables;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Kit.Entity
{
    public static class ConnectionExtensions
    {
        public static DbSet<dynamic> Table(this IDbConnection connection, Type t)
        {
            DbSetContainer container = connection.GetContext().GetDbSet(t);
            return container.DbSet;
        }
        public static DbSet<T> Table<T>(this IDbConnection connection) where T : class
        {
            return connection.GetContext().GetDbSet<T>();
        }

        public static void Insert<T>(this IDbConnection connection, T obj) where T : class
        {
            connection.Table<T>().Add(obj);
        }
        public static void Update<T>(this IDbConnection connection, T obj) where T : class
        {
            connection.GetContext().Update(obj);
        }

        public static void Delete<T>(this IDbConnection connection, Func<T, bool> pred) where T : class
        {
            var delete = connection.Table<T>().Where(pred);
            connection.GetContext().RemoveRange(delete);
        }

        public static void Delete<T>(this IDbConnection connection, T obj) where T : class
        {
            connection.GetContext().Remove(obj);
        }

        public static void DeleteAll<T>(this IDbConnection connection) where T : class
        {
            var table = connection.Table<T>();
            table.RemoveRange(table);
        }

        public static ValueTask<T> FindAsync<T>(this IDbConnection connection, object key) where T : class
        {
            return connection.Table<T>().FindAsync(key);
        }
        public static T Find<T>(this IDbConnection connection, object key) where T : class
        {
            return connection.Table<T>().Find(key);
        }

        public static bool TryToConnect(this IDbConnection connection, string connectionString, out Exception ex)
        {
            string currentConnectionString = connection.ConnectionString;
            if (connection.State.HasFlag(ConnectionState.Open))
                connection.Close();
            connection.ConnectionString = connectionString;
            var result = connection.TryToConnect(out ex);
            if (connection.State.HasFlag(ConnectionState.Open))
                connection.Close();
            connection.ConnectionString = currentConnectionString;
            return result;
        }
        public static bool TryToConnect(this IDbConnection connection, out Exception ex)
        {
            bool valid = false;
            try
            {
                ex = null;
                using (IDbCommand con = connection.BuildCommand("SELECT 1"))
                {
                    con.Connection.Open();
                    valid = (int)con.ExecuteScalar() == 1;
                    con.Connection.Close();
                }
                if (connection.Database == "master" || string.IsNullOrEmpty(connection.Database))
                {
                    throw new DataMisalignedException(
                        "La conexión actual es valida, sin embargo no se especifico una base de datos");
                }
            }
            catch (Exception _ex)
            {
                ex = _ex;
                valid = false;
            }
            return valid;
        }
        public static bool Exists(this IDbConnection connection, string sql, params IDataParameter[] parametros)
        {
            bool result = false;
            connection.Read(sql, (reader) =>
            {
                result = reader.Read();

            }, new CommandConfig() { CommandType = CommandType.Text, ManualRead = true }, parametros);
            return result;
        }
        public static bool TableExists(this IDbConnection connection, string tableName)
        {
            if (connection is SqlConnection sql)
                return SqlServerExtensions.TableExists(sql, tableName);
            if (connection is SqliteConnection lite)
                return SqLiteExtensions.TableExists(lite, tableName);
            throw new NotImplementedException();
        }
        public static bool ViewExists(this IDbConnection connection, string viewName)
        {
            if (connection is SqlConnection sql)
                return SqlServerExtensions.ViewExists(sql, viewName);
            if (connection is SqliteConnection lite)
                return SqLiteExtensions.ViewExists(lite, viewName);
            throw new NotImplementedException();
        }

        public static DatabaseContext GetContext(this IDbConnection connection)
        {
            return DatabaseContainer.Current[connection];
        }

        public static DatabaseTypes GetDatabaseType<T>() where T : IDbConnection => GetDatabaseType(typeof(T));
        public static DatabaseTypes GetDatabaseType(this IDbConnection connection) => GetDatabaseType(connection.GetType());
        public static DatabaseTypes GetDatabaseType(Type type)
        {
            if (type == typeof(SqlConnection))
            {
                return DatabaseTypes.SQLServer;
            }

            if (type == typeof(SqliteConnection))
            {
                return DatabaseTypes.SQLite;
            }
            throw new NotSupportedException();
        }
        public static void InsertOrUpdate<T>(this IDbConnection connection, T obj) where T : class
        {
            var db = connection.GetContext();
            if (db.Entry(obj).State == EntityState.Detached)
                db.GetDbSet<T>().Add(obj);
            // If an immediate save is needed, can be slow though
            // if iterating through many entities:
            db.SaveChanges();
        }

        public static T ExecuteScalar<T>(this IDbConnection connection, string query, params IDataParameter[] args)
        {
            IDbCommand cmd = connection.BuildCommand(query, args);
            return (T)cmd.ExecuteScalar();
        }

        public static DbContextOptions<DatabaseContext> GetContextOptions(this IDbConnection connection)
        {
            DbContextOptionsBuilder<DatabaseContext> builder = new DbContextOptionsBuilder<DatabaseContext>();
            switch (connection.GetDatabaseType())
            {
                case DatabaseTypes.SQLServer:
                    if (string.IsNullOrEmpty(connection?.ConnectionString))
                    {
                        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
                        sqlConnectionStringBuilder.DataSource = ".,1433";
                        sqlConnectionStringBuilder.InitialCatalog = "master";
                        builder.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
                        return builder.Options;
                    }
                    builder.UseSqlServer(connection.ConnectionString);
                    break;
                case DatabaseTypes.SQLite:
                    builder.UseSqlite(connection.ConnectionString);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return builder.Options;
        }

        public static DbSet<T> CreateTable<T>(this IDbConnection connection) where T : class
        {
            return connection.GetContext().GetDbSet<T>();
        }

    }
}
