using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.SqlServer;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Sql.Tables
{
    [Preserve(AllMembers = true)]
    public class SyncVersions
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, MaxLength(50)]
        public string Name { get; set; }
        public int Version { get; set; }

        [Ignore]
        public SyncVersionObject SyncVersionObject
        {
            get => (SyncVersionObject)Type;
            set => Type = (int)value;
        }
        public int Type { get; set; }

        internal static SyncVersions GetVersion(SQLServerConnection connection, string ObjectName, SyncVersionObject trigger)
        {
            int t_value = (int) trigger;
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

        //protected override void CreateTable(SQLServerConnection SQLH)
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
        //public static void SaveVersion(SqlBase SQLH, string tableName)
        //{
        //    SQLH.EXEC($"INSERT INTO DAEMON_VERSION (TABLA,VERSION) VALUES('{tableName}','{Daemon.Current.DaemonConfig.DbVersion}');");
        //}
        public static SyncVersions GetVersion(SqlBase SQLH, TableMapping table)
        {
            if (!SQLH.TableExists(table.TableName))
            {
                return Default(table.TableName);
            }
            return SQLH.Table<SyncVersions>().FirstOrDefault(x => x.Name == table.TableName)
                   ?? Default(table.TableName);
        }
        public static SyncVersions GetVersion(SqlBase SQLH, BaseTableQuery table)
        {
            return GetVersion(SQLH, table.Table);
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
        //protected override void CreateTable(SQLiteConnection SQLH)
        //{
        //    //Just for sqlserver
        //    return;
        //}


        public void Save(SQLServerConnection Connection)
        {
            Connection.Table<SyncVersions>().Delete(x => x.Name == this.Name && x.Type == this.Type);
            Connection.InsertOrReplace(this);
        }
    }
}
