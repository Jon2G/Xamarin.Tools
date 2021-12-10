using Kit.Daemon.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kit.Daemon.Devices;
using Kit.Entity;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Tables;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static Kit.Daemon.Helpers.Helper;
using ITableMapping = System.Data.ITableMapping;

namespace Kit.Daemon.Sync
{
    public class SyncManager : ModelBase
    {
        public const int RegularPackageSize = 100;
        public bool ToDo { get; set; }
        public bool NothingToDo { get => !ToDo; }
        private Queue<ChangesHistory> Pendings;
        private int TotalPendientes;
        public SyncTarget CurrentDirection { get; internal set; }

        public float Progress
        {
            get
            {
                if (TotalPendientes > 0 && Processed > 0)
                {
                    return Processed / (float)TotalPendientes;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int _Processed;

        private int Processed
        {
            get => _Processed;
            set
            {
                _Processed = value;
                Raise(() => this.Progress);
            }
        }

        public int PackageSize { get; private set; }
        private ChangesHistory _CurrentPackage;

        public ChangesHistory CurrentPackage
        {
            get => _CurrentPackage;
            set
            {
                _CurrentPackage = value;
                Raise(() => this.CurrentPackage);
            }
        }

        public SyncManager()
        {
            this.Pendings = new Queue<ChangesHistory>();
            this.Processed = 0;
            this.PackageSize = RegularPackageSize;
            this.CurrentDirection = SyncTarget.NOT_SET;
        }

        public void SetPackageSize(int PackageSize = RegularPackageSize)
        {
            this.PackageSize = PackageSize;
        }

        public bool Download() => GetPendings(SyncTarget.Local);
        public bool Upload() => GetPendings(SyncTarget.Remote);
        private bool GetPendings(SyncTarget SyncTarget)
        {
            try
            {
                Daemon.Current.IsAwake = true;
                if (Daemon.Current.IsSleepRequested)
                {
                    return false;
                }
                this.CurrentPackage = null;
                var source = SyncTarget.InvertDirection();

                if (Daemon.Current.IsSleepRequested)
                {
                    return false;
                }
                var s = Daemon.Current.DaemonConfig[source].Table<SyncHistory>();
                this.Pendings = new Queue<ChangesHistory>(Daemon.Current.DaemonConfig[source]
                    .Table<ChangesHistory>().Where(c => !s.Any(s => c.Guid == s.Guid && s.DeviceId == c.DeviceId))
                    .Take(this.PackageSize <= 0 ? Int32.MaxValue : this.PackageSize)
                    .ToList());

                TotalPendientes = Pendings.Count;
                ToDo = TotalPendientes > 0;
                this.CurrentDirection = SyncTarget;
                if (ToDo && !Daemon.Current.IsSleepRequested)
                {
                    ToDo = ProcesarAcciones(SyncTarget);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.Logger.Error(ex, $"Obteniendo pendientes {SyncTarget}");
            }
            return false;
        }

        private bool ProcesarAcciones(SyncTarget direccion)
        {
            Processed = 0;
            CurrentPackage = null;
            DbSet<dynamic> table = null;
            SyncTarget source = direccion.InvertDirection();
            IDbConnection source_con = Daemon.Current.DaemonConfig[source];
            IDbConnection target_con = Daemon.Current.DaemonConfig[direccion];
            string condition = (source_con is IDbConnection ? "SyncGuid=?" : "SyncGuid=@SyncGuid");

            bool CanDo = false;

            while (Pendings.Any())
            {
                if (Daemon.Current.IsSleepRequested)
                {
                    Pendings.Clear();
                    return false;
                }
                try
                {
                    this.CurrentPackage = Pendings.Dequeue();
                    var tableType = Daemon.Current.Schema[this.CurrentPackage.TableName];
                    table = source_con.Table(tableType);
                    if (table != null)
                    {
                        switch (SyncMode.GetSyncDirection(tableType))
                        {
                            case SyncDirection.TwoWay:
                                break;

                            case SyncDirection.Upload:
                                if (direccion != SyncTarget.Remote)
                                {
                                    CurrentPackage.MarkAsSynced(source_con);
                                    CanDo = true;
                                    continue;
                                }
                                break;

                            case SyncDirection.Download:
                                if (direccion != SyncTarget.Local)
                                {
                                    continue;
                                }
                                break;
                        }
                        //string key = source_con.GetTableMappingKey(this.CurrentPackage.TableName);
                        NotifyTableChangedAction action = CurrentPackage.Action;
                        IEnumerable<dynamic> result = table.Where(x => (x as ISync).Guid == CurrentPackage.Guid);
                        if (!result.Any())
                        {
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (Daemon.Current.IsSleepRequested) { return false; }
                        dynamic i_result = result.First();
                        ISync read = null;
                        if (i_result is ISync isync)
                        {
                            read = isync;
                        }
                        var target_table = target_con.Table(tableType);
                        //ISync read = Convert.ChangeType(i_result, typeof(ISync));
                        if (read is null && action == NotifyTableChangedAction.Delete)
                        {
                            var toDelete = target_table
                                 .Where(x => (x as ISync).Guid == CurrentPackage.Guid);
                            target_table.RemoveRange(toDelete);
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (read is null)
                        {
                            Log.Logger.Warning("READ RESULTO EN NULL '{0}'", this.CurrentPackage.TableName);
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (read != null && CurrentPackage is not null)
                        {
                            switch (CurrentPackage.Action)
                            {
                                case NotifyTableChangedAction.Insert:
                                case NotifyTableChangedAction.Update:

                                    if (direccion == SyncTarget.Local || read.ShouldSync(source_con, target_con))
                                    {
                                        CanDo = true;
                                        object old_pk = read.GetPk();


                                        if (read.CustomUpload(source_con, target_con, table))
                                        {
                                            CurrentPackage.MarkAsSynced(source_con);
                                            Processed++;
                                            read.OnSynced(direccion, action);
                                            return true;
                                        }
                                        target_con.InsertOrUpdate(read);

                                        //if (target_con is IDbConnection)
                                        //{
                                          
                                        //}
                                        //else
                                        //{
                                        //    target_con.Table<ChangesHistory>().Delete(x => x.Guid == CurrentPackage.Guid);
                                        //    target_con.Insert(read, String.Empty, read.GetType(), false);
                                        //}

                                        if (source_con is IDbConnection lite)
                                        {
                                            if (read.Affects(lite, old_pk))
                                            {
                                                CurrentPackage.MarkAsSynced(source_con);
                                                Processed++;
                                                read.OnSynced(direccion, action);
                                                return true;
                                            }
                                        }

                                        CurrentPackage.MarkAsSynced(source_con);
                                        Processed++;
                                        read.OnSynced(direccion, action);
                                    }
                                    break;

                                case NotifyTableChangedAction.Delete:
                                    read.Delete(source_con, target_con, table);
                                    read.OnSynced(direccion, action);
                                    CurrentPackage.MarkAsSynced(source_con);
                                    Processed++;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Log.Logger.Warning("TABLA NO ENCONTRADA EN EL SCHEMA DEFINIDO '{0}'", this.CurrentPackage.TableName);
                        CurrentPackage.MarkAsSynced(source_con);
                        Processed++;
                    }
                }
                catch (Exception ex)
                {
                    //if (ex is SQLiteException)
                    //{
                    //    if (ex.Message.Contains("no such table"))
                    //    {
                    //        target_con.CreateTable(table);
                    //    }
                    //}
                    if (Debugger.IsAttached)
                    {
                        //Debugger.Break();
                    }
                    Log.Logger.Error(ex, "Al sincronizar - {0}", CurrentPackage);
                }
            }

            return CanDo;
        }

        private string BuildSqlServerQuery()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            if (PackageSize > 0)
            {
                sb.Append("TOP ").Append(PackageSize);
            }
            sb.Append(@" ID,ACCION,TABLA,LLAVE FROM VERSION_CONTROL WHERE NOT EXISTS(SELECT ID_DISPOSITIVO FROM DESCARGAS_VERSIONES WHERE DESCARGAS_VERSIONES.ID_DESCARGA = VERSION_CONTROL.ID AND ID_DISPOSITIVO ='")
                .Append(Device.Current.DeviceId).Append("') --ORDER BY TABLA DESC, LLAVE ASC;");
            return sb.ToString();
        }

        public void Reset()
        {
            this.Pendings.Clear();
            this.ToDo = false;
        }
    }
}