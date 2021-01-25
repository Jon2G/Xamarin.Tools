using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Kit.Daemon.Abstractions
{
    public class Trigger
    {
        public static void CheckTrigger(SQLH SQLH, Table Table, string DbVersion)
        {
            try
            {
                string TriggerName = $"{Table.Name}_TRIGGER";
                string version = SQLH.Single<string>("select VERSION from TRIGGERS_INFO WHERE NAME=@NAME"
                     , false, System.Data.CommandType.Text, new SqlParameter("NAME", TriggerName));

                if (version != DbVersion)
                {
                    if (SQLH.TriggerExists(TriggerName))
                    {
                        SQLH.EXEC($"DROP TRIGGER {TriggerName}", System.Data.CommandType.Text, false);
                    }

                    //REMOVE OLD TRIGGERS
                    if (SQLH.TriggerExists($"{Table.Name}_Tablet"))
                    {
                        SQLH.EXEC($"DROP TRIGGER {Table.Name}_Tablet");
                    }

                    SQLH.EXEC($@"CREATE TRIGGER dbo.{TriggerName} ON dbo.{Table.Name} AFTER INSERT,DELETE,UPDATE AS 
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
   ON I.{Table.PrimaryKey}=V.LLAVE
   WHERE 
   (V.ACCION='U' OR V.ACCION='I')  AND
   V.TABLA='{Table.Name}'  AND
   V.CAMPO='{Table.PrimaryKey}'
   INSERT INTO VERSION_CONTROL(ACCION,LLAVE,CAMPO,TABLA) 
   SELECT @action,{Table.PrimaryKey},'{Table.PrimaryKey.ToUpper()}','{Table.Name.ToUpper()}'
   FROM inserted
END ELSE
BEGIN
   DELETE V
   FROM VERSION_CONTROL V
   INNER JOIN deleted D
   ON D.{Table.PrimaryKey}=V.LLAVE
   WHERE 
   V.TABLA='{Table.Name}' AND
   V.CAMPO='{Table.PrimaryKey}'
   INSERT INTO VERSION_CONTROL(ACCION,LLAVE,CAMPO,TABLA) 
   SELECT @action,{Table.PrimaryKey},'{Table.PrimaryKey.ToUpper()}','{Table.Name.ToUpper()}'
   FROM deleted
END
END", System.Data.CommandType.Text, false);

                    SQLH.EXEC($"UPDATE {Table.Name} SET {Table.Fields[Table.Fields.Length - 1]}={Table.Fields[Table.Fields.Length - 1]}", System.Data.CommandType.Text, false);

                    SQLH.EXEC($"DELETE FROM TRIGGERS_INFO WHERE NAME=@NAME", System.Data.CommandType.Text, false
                        , new SqlParameter("NAME", TriggerName));

                    SQLH.EXEC($"INSERT INTO TRIGGERS_INFO (NAME,VERSION) VALUES(@NAME,@VERSION)", System.Data.CommandType.Text, false
                        , new SqlParameter("NAME", TriggerName)
                        , new SqlParameter("VERSION", DbVersion)
                        );

                }
                //Table.IsTriggerChecked = true;
            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex, $"Al revisar el trigger de {Table.Name}");
            }
        }
    }
}
