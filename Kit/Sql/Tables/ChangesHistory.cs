using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;
using Kit.Daemon.Sync;
using Kit.Entity;
using Kit.Sql.Attributes;
using System.Data.Entity.Migrations;
using Kit.Sql.Enums;


namespace Kit.Sql.Tables
{
    [Preserve(AllMembers = true)]
    /// <summary>
    /// A table that keeps track of every change made on sqlite databate
    /// </summary>
    public class ChangesHistory : ISync
    {
        [Key, Index(IsClustered = true, IsUnique = true), Column("SyncGuid")]
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
        [MaxLength(100)]
        public string DeviceId { get; set; }
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

        public void Save(IDbConnection con)
        {
            con.InsertOrUpdate(this);
        }

        public static void MarkAsSynced(IDbConnection origin, Guid SyncGuid)
        {
            SyncHistory syncHistory = new SyncHistory
            {
                DeviceId = Daemon.Devices.Device.Current.DeviceId,
                Guid = SyncGuid
            };
            origin.Delete<SyncHistory>(x => x.Guid == syncHistory.Guid);
            origin.Insert(syncHistory);
        }

        public static void MarkAsSynced(IDbConnection origin, ISync ISync) => MarkAsSynced(origin, ISync.Guid);

        public void MarkAsSynced(IDbConnection origin)
        {
            try
            {
                SyncHistory syncHistory = new SyncHistory
                {
                    DeviceId = Daemon.Devices.Device.Current.DeviceId,
                    Guid = this.Guid,
                    Date = DateTime.Now
                };
                origin.Delete<SyncHistory>(x => x.Guid == syncHistory.Guid);
                origin.Insert(syncHistory);
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
    }
}