using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using Kit.Entity;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ITableMapping = System.Data.ITableMapping;

namespace Kit.Sql.Tables
{
    [Preserve(AllMembers = true)]
    public class SyncVersions
    {
        [Key, Index(IsClustered = true, IsUnique = true)]
        public int Id { get; set; }
        [Index(IsClustered = true, IsUnique = true), MaxLength(50)]
        public string Name { get; set; }
        public int Version { get; set; }

        [NotMapped]
        public SyncVersionObject SyncVersionObject
        {
            get => (SyncVersionObject)Type;
            set => Type = (int)value;
        }
        public int Type { get; set; }

        internal static SyncVersions GetVersion(IDbConnection connection, string ObjectName, SyncVersionObject trigger)
        {
            int t_value = (int)trigger;
            SyncVersions version =
                connection.Table<SyncVersions>().FirstOrDefault(x => x.Name == ObjectName && x.Type == t_value);
            if (version is null)
            {
                version = new SyncVersions()
                {
                    Name = ObjectName,
                    SyncVersionObject = trigger
                };
            }
            return version;
        }

        public SyncVersions() { }

        //protected override void CreateTable(IDbConnection SQLH)
        //{
        //    if (SQLH.TableExists("DESCARGAS_VERSIONES"))
        //        SQLH.EXEC("DROP TABLE DESCARGAS_VERSIONES");

        //    if (SQLH.TableExists("DISPOSITVOS_TABLETS"))
        //        SQLH.EXEC("DROP TABLE DISPOSITVOS_TABLETS");

        //    if (SQLH.TableExists("VERSION_CONTROL"))
        //        SQLH.EXEC("DROP TABLE VERSION_CONTROL");

        //    if (SQLH.TableExists("TRIGGERS_INFO"))
        //        SQLH.EXEC("DROP TABLE TRIGGERS_INFO");

        //    SQLH.EXEC(@"CREATE TABLE DAEMON_VERSION (
        //          ID INT IDENTITY(1,1) PRIMARY KEY,
        //          TABLA VARCHAR(100) DEFAULT '',
        //          VERSION VARCHAR(100) DEFAULT '0.0.0');");

        //}
        //public static void SaveVersion(IDbConnection SQLH, string tableName)
        //{
        //    SQLH.EXEC($"INSERT INTO DAEMON_VERSION (TABLA,VERSION) VALUES('{tableName}','{Daemon.Current.DaemonConfig.DbVersion}');");
        //}
        public static SyncVersions GetVersion(IDbConnection SQLH, ITableMapping table)
        {
            //TODO
            //if (!SQLH.TableExists(table.TableName))
            //{
            //    return Default(table.TableName);
            //}
            //return SQLH.Table<SyncVersions>().FirstOrDefault(x => x.Name == table.TableName)
            //       ?? Default(table.TableName);
            return null;
        }
        public static SyncVersions GetVersion(IDbConnection SQLH, Type tableType)
        {
            //TODO
            return null;
            //return GetVersion(SQLH, table.Table);
        }

        private static SyncVersions Default(string TableName)
        {
            return new SyncVersions()
            {
                Name = TableName,
                SyncVersionObject = SyncVersionObject.Table,
                Version = 0
            };
        }
        //protected override void CreateTable(IDbConnection SQLH)
        //{
        //    //Just for sqlserver
        //    return;
        //}


        public void Save(IDbConnection Connection)
        {
            Connection.Delete<SyncVersions>(x => x.Name == this.Name && x.Type == this.Type);
            Connection.InsertOrUpdate(this);
        }
    }
}
