using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Kit.Sql.Base;
using SQLServer;

namespace Kit.Daemon.Abstractions
{
    public class Trigger
    {
        public static void CheckTrigger(SQLServerConnection Connection, TableMapping Table, int DbVersion)
        {
            try
            {
                string TriggerName = $"{Table.TableName}_TRIGGER";
                int version = Connection.Single<int>("select VERSION from TRIGGERS_INFO WHERE NAME=@NAME"
                     , new SqlParameter("NAME", TriggerName));

                if (version != DbVersion)
                {
                    if (Connection.TriggerExists(TriggerName))
                    {
                        Connection.EXEC($"DROP TRIGGER {TriggerName}", System.Data.CommandType.Text);
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
    DECLARE @action as char(1);
    SET @action = 'I'; -- Set Action to Insert by default.
    IF EXISTS(SELECT * FROM DELETED)
    BEGIN
        SET @action = 
            CASE
                WHEN EXISTS(SELECT * FROM INSERTED) THEN 'U' -- Set Action to Updated.
                ELSE 'D' -- Set Action to Deleted.       
            END
    END
    ELSE 
        IF NOT EXISTS(SELECT * FROM INSERTED) 
		RETURN;

IF @ACTION='I' OR @action='U'
BEGIN
   DELETE V
   FROM VERSION_CONTROL V
   INNER JOIN inserted I
   ON I.{Table.PK.Name}=V.LLAVE
   WHERE 
   (V.ACCION='U' OR V.ACCION='I')  AND
   V.TABLA='{Table.TableName}'  AND
   V.CAMPO='{Table.PK.Name}'
   INSERT INTO VERSION_CONTROL(ACCION,LLAVE,CAMPO,TABLA) 
   SELECT @action,{Table.PK.Name},'{Table.PK.Name.ToUpper()}','{Table.TableName.ToUpper()}'
   FROM inserted
END ELSE
BEGIN
   DELETE V
   FROM VERSION_CONTROL V
   INNER JOIN deleted D
   ON D.{Table.PK.Name}=V.LLAVE
   WHERE 
   V.TABLA='{Table.TableName}' AND
   V.CAMPO='{Table.PK.Name}'
   INSERT INTO VERSION_CONTROL(ACCION,LLAVE,CAMPO,TABLA) 
   SELECT @action,{Table.PK.Name},'{Table.PK.Name.ToUpper()}','{Table.TableName.ToUpper()}'
   FROM deleted
END
END", System.Data.CommandType.Text);

                   Connection.EXEC($"UPDATE {Table.TableName} SET {Table.Columns.Last().Name}={Table.Columns.Last().Name}", System.Data.CommandType.Text);

                    Connection.EXEC($"DELETE FROM TRIGGERS_INFO WHERE NAME=@NAME", System.Data.CommandType.Text
                        , new SqlParameter("NAME", TriggerName));

                    Connection.EXEC($"INSERT INTO TRIGGERS_INFO (NAME,VERSION) VALUES(@NAME,@VERSION)", System.Data.CommandType.Text
                        , new SqlParameter("NAME", TriggerName)
                        , new SqlParameter("VERSION", DbVersion)
                        );

                }
                //Table.IsTriggerChecked = true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Al revisar el trigger de {Table.TableName}");
            }
        }
    }
}
