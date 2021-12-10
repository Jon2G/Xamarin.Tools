using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using ClosedXML;
using Kit.SetUpConnectionString;
using Kit.Sql.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace Kit.Entity.Abstractions
{
    public enum DatabaseTypes { SQLServer, SQLite }
    public class DatabaseContext : DbContext
    {
        public readonly DatabaseTypes DatabaseType;
        private readonly Dictionary<Type, DbSetContainer> DbSets;
        public readonly IDbConnection DbConnection;
        public dynamic this[Type type] => GetDbSet(type);
        public dynamic this[string tableName] => GetDbSet(tableName);
        public Type[] DbTypes { get; private set; }

        public DbSetContainer GetDbSet(Type type)
        {
            return DbSets[type];
        }

        public Microsoft.EntityFrameworkCore.DbSet<T> GetDbSet<T>() where T : class
        {
            DbSetContainer<T> dbset = (DbSetContainer<T>)GetDbSet(typeof(T));
            return (Microsoft.EntityFrameworkCore.Internal.InternalDbSet<T>)dbset.DbSet;
        }

        public dynamic GetDbSet(string tableName) => this.DbSets.First(x => x.Value.TableName == tableName);

        public readonly uint Version;
        public DatabaseContext(string dbName, IDbConnection connection, uint version, DbContextOptions<DatabaseContext> options)
            : base(connection.GetContextOptions())
        {
            this.Version = version;
            this.DbConnection = connection;
            this.DatabaseType = this.DbConnection.GetDatabaseType();
            this.DbSets = new Dictionary<Type, DbSetContainer>();
        }

        public DatabaseContext(string dbName, IDbConnection connection, uint version) :
            this(dbName, connection, version, new DbContextOptions<DatabaseContext>())
        {

        }
        public DatabaseContext(string dbName, IDbConnection connection) :
            this(dbName, connection, 0, new DbContextOptions<DatabaseContext>())
        {

        }
        public DatabaseContext Build(Type[] types)
        {
            this.DbTypes = types;
            var methodInfo = this.GetType().GetMethod(nameof(Set));
            HashSet<DbSetContainer> dbSets = new();
            foreach (Type type in types)
            {
                MethodInfo genericMethod = methodInfo.MakeGenericMethod(new Type[] { type });
                _ = genericMethod.Invoke(this, null);
            }
            return this;
        }

        public void CheckTableExistsAndCreateIfMissing(Type type)
        {
            var tableName = GetTableName(type);
            if (!this.DbConnection.TableExists(tableName))
            {
                var scriptStart = $"CREATE TABLE";
                const string scriptEnd = "GO";
                var script = GenerateCreateScript();
                var tableScript = script.Split(new string[] { scriptStart }, StringSplitOptions.RemoveEmptyEntries)
                    .Last().Split(new string[] { scriptEnd }, StringSplitOptions.RemoveEmptyEntries);
                string first = tableScript.First();
                if (!first.StartsWith(scriptStart))
                {
                    first = $"{scriptStart} {first}";
                }
                Database.ExecuteSqlRaw(first);
                Log.Logger.Debug($"Database table: '{tableName}' was created.");
            }
        }
        public void CheckTableExistsAndCreateIfMissing<T>() where T : class => CheckTableExistsAndCreateIfMissing(typeof(T));
        public string GenerateCreateScript()
        {
            var model = Database.GetService<IModel>();
            var migrationsModelDiffer = Database.GetService<IMigrationsModelDiffer>();
            var migrationsSqlGenerator = Database.GetService<IMigrationsSqlGenerator>();
            var sqlGenerationHelper = Database.GetService<ISqlGenerationHelper>();
#if !NET48
            var operations = migrationsModelDiffer.GetDifferences(null, (IRelationalModel)model);
#else
            var operations = migrationsModelDiffer.GetDifferences(null, model);
#endif
            var commands = migrationsSqlGenerator.Generate(operations, model);

            var stringBuilder = new StringBuilder();
            foreach (var command in commands)
            {
                stringBuilder
                    .Append(command.CommandText)
                    .AppendLine(sqlGenerationHelper.BatchTerminator);
            }

            return stringBuilder.ToString();
        }
        public override Microsoft.EntityFrameworkCore.DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            Microsoft.EntityFrameworkCore.DbSet<TEntity> dbSet = base.Set<TEntity>();
            DbSetContainer container = DbSetContainer.Contain(dbSet);
            this.DbSets.Add(typeof(TEntity), container);
            return dbSet;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (Type dbType in DbTypes)
            {
                var methodInfo = typeof(ModelBuilder).GetMethods().FirstOrDefault(x => x.IsGenericMethod && x.Name == nameof(ModelBuilder.Entity));
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { dbType });
                var obj = methodInfo.Invoke(modelBuilder, null);
                methodInfo = typeof(RelationalEntityTypeBuilderExtensions).GetMethod(nameof(RelationalEntityTypeBuilderExtensions.ToTable)
                    , BindingFlags.Static | BindingFlags.Public
                    , null
                    , new Type[] { typeof(EntityTypeBuilder), typeof(string) }
                    , null);
                obj = methodInfo.Invoke(null, new object[] { obj, GetTableName(dbType) });
            }


        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            return base.Update(entity);
        }

        internal void CheckTables(Type[] tables)
        {
            var databaseCreator = (Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator);
            databaseCreator.CreateTables();
        }

        public static string GetTableName(Type type)
        {
            var table = (TableAttribute)type.GetTypeInfo().GetCustomAttribute(typeof(TableAttribute));
            if (table is null)
            {

            }
            return table.Name;
        }
    }
}
