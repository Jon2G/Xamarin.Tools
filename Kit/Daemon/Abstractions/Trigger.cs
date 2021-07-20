using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kit.Daemon.Sync;
using Kit.Sql.Enums;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Daemon.Abstractions
{
    public static class Trigger
    {
        public static void CheckTrigger(SQLServerConnection Connection, TableMapping Table, int DbVersion)
        {
            int Timeout = 30;
            try
            {
                Timeout = Connection.CommandTimeout;
                Connection.CommandTimeout = (int)TimeSpan.FromMinutes(9).TotalMinutes;

                string TriggerName = $"{Table.TableName}_TRIGGER";

                bool TriggerExists = Connection.TriggerExists(TriggerName);

                SyncVersions version =
                    TriggerExists ?
                    SyncVersions.GetVersion(Connection, TriggerName, SyncVersionObject.Trigger) :
                    new SyncVersions()
                    {
                        Name = TriggerName,
                        SyncVersionObject = SyncVersionObject.Trigger
                    };

                if (!TriggerExists || !Connection.TableExists(Table.TableName) || version.Version != DbVersion)
                {
                    Connection.CreateTable(Table);
                    if (TriggerExists)
                    {
                        Connection.EXEC($"DROP TRIGGER {TriggerName}", System.Data.CommandType.Text);
                    }

                    string clustered_index_name = $"IX_{Table.TableName}_{ISync.SyncGuidColumnName}";
                    if (!Connection.ExistsClusteredIndex(Table.TableName, clustered_index_name))
                    {
                        Connection.CreateClusteredIndex(Table.TableName, ISync.SyncGuidColumnName, clustered_index_name);
                    }

                    //REMOVE OLD TRIGGERS
                    if (Connection.TriggerExists($"{Table.TableName}_Tablet"))
                    {
                        Connection.EXEC($"DROP TRIGGER {Table.TableName}_Tablet");
                    }

                    Connection.EXEC($@"CREATE TRIGGER dbo.{TriggerName} ON dbo.{Table.TableName} AFTER INSERT,DELETE,UPDATE AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- Check if this is an INSERT, UPDATE or DELETE Action.
    DECLARE @action as char(10);
    SET @action = '{NotifyTableChangedAction.Insert}'; -- Set Action to Insert by default.
    IF EXISTS(SELECT * FROM DELETED)
    BEGIN
        SET @action =
            CASE
                WHEN EXISTS(SELECT * FROM INSERTED) THEN '{NotifyTableChangedAction.Update}' -- Set Action to Updated.
                ELSE '{NotifyTableChangedAction.Delete}' -- Set Action to Deleted.
            END
    END
    ELSE
        IF NOT EXISTS(SELECT * FROM INSERTED)
		RETURN;

IF @ACTION='{NotifyTableChangedAction.Insert}' OR @action='{NotifyTableChangedAction.Update}'
BEGIN
DELETE FROM SyncHistory WHERE SyncGuid IN ( SELECT SyncGuid FROM deleted)
DELETE FROM ChangesHistory WHERE SyncGuid IN ( SELECT SyncGuid FROM deleted)

INSERT INTO ChangesHistory (Action,SyncGuid,TableName,Priority)
SELECT @action,SyncGuid,'{Table.TableName}',{Table.SyncMode.Order} FROM inserted

END ELSE
BEGIN
DELETE FROM SyncHistory WHERE SyncGuid IN ( SELECT SyncGuid FROM deleted)
DELETE FROM ChangesHistory WHERE SyncGuid IN ( SELECT SyncGuid FROM deleted)

INSERT INTO ChangesHistory (Action,SyncGuid,TableName,Priority)
SELECT @action,SyncGuid,'{Table.TableName}',{Table.SyncMode.Order} FROM deleted

END
END", System.Data.CommandType.Text);

                    var last = Table.Columns.LastOrDefault();
                    if (last != null)
                        using (var con = Connection.Con())
                        {
                            using (var cmd = new System.Data.SqlClient.SqlCommand($"UPDATE {Table.TableName} SET SyncGuid=ISNULL(SyncGuid,NEWID())", con)
                            {
                                CommandTimeout = Int32.MaxValue
                            })
                            {
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                    version.Version = DbVersion;
                    version.Save(Connection);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Al revisar el trigger de {Table.TableName}");
            }
            finally
            {
                Connection.CommandTimeout = Timeout;
            }
        }
    }
}