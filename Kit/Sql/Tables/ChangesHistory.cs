using Kit.Daemon.Abstractions;
using Kit.Daemon.Enums;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TableMapping = Kit.Sql.SqlServer.TableMapping;

namespace Kit.Sql.Tables
{
    [Preserve(AllMembers = true)]
    /// <summary>
    /// A table that keeps track of every change made on sqlite databate
    /// </summary>
    public class ChangesHistory : ISync
    {
        [PrimaryKey, AutoIncrement, Column("SyncGuid")]
        public override Guid Guid { get; set; }

        /// <summary>
        /// Name of the table where te change has been made
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Type of change
        /// </summary>
        public NotifyTableChangedAction Action { get; set; }

        public int Priority { get; set; }

        public DateTime Date { get; set; }

        public ChangesHistory()
        {
        }

        public ChangesHistory(string TableName, Guid SyncGuid, NotifyTableChangedAction Action, int Priority)
        {
            this.TableName = TableName;
            this.Guid = SyncGuid;
            this.Action = Action;
            this.Priority = Priority;
            this.Date = DateTime.Now;
        }

        public void Save(SQLiteConnection con)
        {
            con.InsertOrReplace(this, false);
        }

        public static void MarkAsSynced(SqlBase origin, Guid SyncGuid)
        {
            SyncHistory syncHistory = new SyncHistory
            {
                DeviceId = Daemon.Devices.Device.Current.DeviceId,
                Guid = SyncGuid
            };
            origin.Table<SyncHistory>().Delete(x => x.Guid == syncHistory.Guid);
            origin.Insert(syncHistory, string.Empty);
        }

        public static void MarkAsSynced(SqlBase origin, ISync ISync) => MarkAsSynced(origin, ISync.Guid);

        public void MarkAsSynced(SqlBase origin)
        {
            try
            {
                SyncHistory syncHistory = new SyncHistory
                {
                    DeviceId = Daemon.Devices.Device.Current.DeviceId,
                    Guid = this.Guid,
                    Date = DateTime.Now
                };
                origin.Table<SyncHistory>().Delete(x => x.Guid == syncHistory.Guid);
                origin.Insert(syncHistory, string.Empty);
                this.SyncStatus = Daemon.Enums.SyncStatus.Done;
                //if (connection is SqlServer SQLH)
                //{
                //    SQLH.EXEC("INSERT INTO DESCARGAS_VERSIONES(ID_DESCARGA,ID_DISPOSITIVO) VALUES(@ID_DESCARGA,@ID_DISPOSITIVO)"
                //            , System.Data.CommandType.Text, false,
                //            new SqlParameter("ID_DESCARGA", Id),
                //            new SqlParameter("ID_DISPOSITIVO", Device.Current.DeviceId));
                //}
                //else if (connection is SqLite SQLHLite)
                //{
                //    SQLHLite.EXEC($"DELETE FROM VERSION_CONTROL WHERE ID=?", Id);
                //}
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al marcar como finalizada la sincronización - [{0}]", this);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                switch (this.Action)
                {
                    case NotifyTableChangedAction.Update:
                        sb.Append("Actualizando");
                        break;

                    case NotifyTableChangedAction.Insert:
                        sb.Append("Descargando");
                        break;

                    case NotifyTableChangedAction.Delete:
                        sb.Append("Eliminando");
                        break;

                    default:
                        sb.Append("NONE");
                        break;
                }

                sb.Append(" ");
                switch (this.TableName?.ToUpper())
                {
                    case "LINEAS":
                        sb.Append("lineas");
                        break;

                    case "PRODS":
                        sb.Append("productos");
                        break;

                    case "ALMACEN":
                        sb.Append("almacenes");
                        break;

                    case "CLAVESADD":
                        sb.Append("códigos de barras");
                        break;

                    default:
                        sb.Append(this.TableName);
                        break;
                }

                sb.Append(" [").AppendFormat("{0:N}", Guid).Append("]");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al convertir este pendiete en su representación ToString");
            }

            return sb.ToString();
        }

        public virtual ISync? GetDeletedObjInfo(SyncManager manager, SqlBase source_con, SyncTarget target, SchemaTable? schemaTable)
        {
            ISync? result = null;
            Base.TableMapping? mapping = schemaTable?.For(source_con);
            if (mapping is null)
            {
                return result;
            }
            result = Activator.CreateInstance(mapping.MappedType) as ISync;
            if (result is null)
            {
                return result;
            }

            result.Guid = this.Guid;
            result.SyncStatus = this.SyncStatus;
            return result;
        }
        public virtual dynamic? GetObject(SyncManager manager, SqlBase source_con, SyncTarget target, SchemaTable? schemaTable)
        {
            dynamic? result = null;
            string condition = (source_con is SQLiteConnection ? $"SyncGuid='{this.Guid}'" : "SyncGuid=@SyncGuid");
            if (schemaTable is not null)
            {
                switch (schemaTable.SyncDirection)
                {
                    case SyncDirection.TwoWay:
                        break;

                    case SyncDirection.Upload:
                        if (target != SyncTarget.Remote)
                        {
                            manager.CurrentPackage.MarkAsSynced(source_con);
                            return result;
                        }
                        break;

                    case SyncDirection.Download:
                        if (target != SyncTarget.Local)
                        {
                            return result;
                        }
                        break;
                }

                //string key = source_con.GetTableMappingKey(this.CurrentPackage.TableName);
                NotifyTableChangedAction action = this.Action;
                var table = schemaTable.For(source_con);
                string selection_list = table.SelectionList;
                CommandBase command = source_con.CreateCommand(
                    $"SELECT {selection_list} FROM {table.TableName} WHERE {condition}",
                    new BaseTableQuery.Condition("SyncGuid", this.Guid));

                DaemonCompiledSetter compiledSetter = schemaTable.CompiledSetterFor(source_con);
                MethodInfo method = command.GetType().GetMethod(
                    compiledSetter is null
                        ? "ExecuteDeferredQueryAndCompile"
                        : nameof(CommandBase.ExecuteDeferredQuery),
                    compiledSetter is null
                        ? new[] { typeof(TableMapping) }
                        : new[] { typeof(TableMapping), typeof(DaemonCompiledSetter) });
                method = method.MakeGenericMethod(table.MappedType);
                if (compiledSetter is null)
                {
                    DaemonCompiledSetterTuple resultCompiled =
                        (DaemonCompiledSetterTuple)method.Invoke(command, new object[] { table });
                    if (resultCompiled?.Results?.Any() ?? false)
                    {
                        compiledSetter = resultCompiled.CompiledSetter;
                        schemaTable.Add(table, compiledSetter);
                    }
                    result = resultCompiled?.Results?.ToList()?.FirstOrDefault();
                }
                else
                {
                    result = ((IEnumerable<dynamic>)method.Invoke(command, new object[] { table, compiledSetter }))
                        ?.ToList()?.FirstOrDefault();
                }
            }
            return result;
        }
    }
}