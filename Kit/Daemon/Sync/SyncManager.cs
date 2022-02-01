using Kit.Daemon.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kit.Daemon.Devices;
using Kit.Model;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Exceptions;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;
using static Kit.Daemon.Helpers.Helper;
using TableMapping = Kit.Sql.Base.TableMapping;
using Kit.Daemon.Abstractions;

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

        private string DownloadQuery
        {
            get;
            set;
        }

        private string UploadQuery
        {
            get;
            set;
        }

        private Dictionary<TableMapping, DaemonCompiledSetter> CompiledSetters { get; set; }
        public SyncManager()
        {
            this.CompiledSetters = new Dictionary<TableMapping, DaemonCompiledSetter>();
            this.Pendings = new Queue<ChangesHistory>();
            this.Processed = 0;
            this.PackageSize = RegularPackageSize;
            this.DownloadQuery = string.Empty;
            this.CurrentDirection = SyncTarget.NOT_SET;
        }

        public void SetPackageSize(int PackageSize = RegularPackageSize)
        {
            this.PackageSize = PackageSize;
            UploadQuery = null;
            DownloadQuery = string.Empty;
        }

        public bool Download()
        {
            lock (DownloadQuery)
            {
                if (string.IsNullOrEmpty(DownloadQuery))
                {
                    DownloadQuery = PrepareQuery(Daemon.Current.DaemonConfig[SyncTarget.Remote]);
                    Log.Logger.Information("Prepared {0} Download Query - [{1}]", "DAEMON", DownloadQuery);
                }
                return GetPendings(SyncTarget.Local);
            }
        }

        public bool Upload()
        {
            if (UploadQuery is null)
            {
                UploadQuery = PrepareQuery(Daemon.Current.DaemonConfig[SyncTarget.Local]);
                Log.Logger.Information("Prepared {0} Upload Query - [{1}]", "DAEMON", UploadQuery);
            }
            return GetPendings(SyncTarget.Remote);
        }

        private string PrepareQuery(SqlBase source)
        {
            string query = string.Empty;
            switch (source)
            {
                case SQLServerConnection:
                    query = "SELECT ";
                    if (PackageSize > 0)
                    {
                        query += $"TOP {this.PackageSize}";
                    }
                    query += $" SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid) order by Priority";
                    return query;

                case SQLiteConnection:
                    query =
                        $"select SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid) order by Priority";
                    if (PackageSize > 0)
                    {
                        query += $" limit {this.PackageSize}";
                    }
                    return query;
            }
            return query;
        }

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
                string query = string.Empty;
                switch (SyncTarget)
                {
                    case SyncTarget.Local:
                        query = DownloadQuery;
                        break;

                    case SyncTarget.Remote:
                        query = UploadQuery;
                        break;
                }
                var source = SyncTarget.InvertDirection();
                if (!string.IsNullOrEmpty(query))
                {
                    if (Daemon.Current.IsSleepRequested)
                    {
                        return false;
                    }
                    this.Pendings = new Queue<ChangesHistory>(Daemon.Current.DaemonConfig[source].RenewConnection()
                        .DeferredQuery<ChangesHistory>(query).ToList());
                }
                else
                {
                    return GetPendings(SyncTarget);
                }
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
        public bool ProcesarAcciones(SyncTarget direccion, ISync read, SqlBase target_con, SqlBase source_con, NotifyTableChangedAction action)
        {
            SchemaTable schemaTable = Daemon.Current.Schema[this.CurrentPackage.TableName, direccion];
            TableMapping table = schemaTable.For(source_con);

            bool CanDo = false;
            //ISync read = Convert.ChangeType(i_result, typeof(ISync));
            if (read is null && action == NotifyTableChangedAction.Delete)
            {
                if (target_con is SQLiteConnection lite)
                {
                    lite.EXEC($"DELETE FROM {table.TableName} where SyncGuid=?", CurrentPackage.Guid);
                }
                else if (target_con is SQLServerConnection con)
                {
                    con.EXEC($"DELETE FROM {table.TableName} where SyncGuid=@SyncGuid", new System.Data.SqlClient.SqlParameter("SyncGuid", CurrentPackage.Guid));
                }
                CurrentPackage.MarkAsSynced(source_con);
                return CanDo;
            }
            if (read is null)
            {
                Log.Logger.Warning("READ RESULTO EN NULL '{0}'", this.CurrentPackage.TableName);
                CurrentPackage.MarkAsSynced(source_con);
                return CanDo;
            }
            if (read != null && CurrentPackage is not null)
            {
                switch (CurrentPackage.Action)
                {
                    case NotifyTableChangedAction.Insert:
                    case NotifyTableChangedAction.Update:
                        bool canSync = false;
                        if (direccion == SyncTarget.Remote)
                        {
                            bool shouldSync = read.ShouldSync(source_con, target_con);
                            canSync = shouldSync;
                        }
                        else if (direccion == SyncTarget.Local)
                        {
                            canSync = true;
                        }

                        if (canSync)
                        {
                            CanDo = true;
                            object old_pk = null;

                            if (table.PK != null)
                            {
                                old_pk = read.GetPk();
                            }

                            if (read.CustomUpload(source_con, target_con, table))
                            {
                                CurrentPackage.MarkAsSynced(source_con);
                                Processed++;
                                read.OnSynced(direccion, action);
                                return true;
                            }

                            if (target_con is SQLiteConnection)
                            {
                                target_con.InsertOrReplace(read, false);
                            }
                            else
                            {
                                target_con.Table<ChangesHistory>().Delete(x => x.Guid == CurrentPackage.Guid);
                                //if (target_con.Insert(read, String.Empty, read.GetType(), false) <= 0)
                                if (target_con.InsertOrReplace(read, false) <= 0)
                                {
                                    Processed++;
                                    read.OnSyncFailed(target_con,source_con,target_con.LastException);
                                    return CanDo;
                                }
                            }

                            if (source_con is SQLiteConnection lite)
                            {
                                if (read.Affects(this, lite, old_pk))
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
                        else { Processed++; }
                        break;

                    case NotifyTableChangedAction.Delete:
                        read.Delete(source_con, target_con, table);
                        read.OnSynced(direccion, action);
                        CurrentPackage.MarkAsSynced(source_con);
                        Processed++;
                        break;
                }
            }
            return CanDo;
        }
        public bool ProcesarAcciones(SyncTarget direccion, SqlBase target_con, SqlBase source_con, NotifyTableChangedAction action, params ISync[] syncObjs)
        {
            bool result = true;
            foreach (ISync sync in syncObjs)
            {
                if (!ProcesarAcciones(direccion, sync, target_con, source_con, action))
                {
                    result = false;
                }
            }
            return result;
        }
        private bool ProcesarAcciones(SyncTarget direccion)
        {
            Processed = 0;
            CurrentPackage = null;
            SchemaTable schemaTable = null;
            TableMapping table = null;
            SyncTarget source = direccion.InvertDirection();
            SqlBase source_con = Daemon.Current.DaemonConfig[source];
            SqlBase target_con = Daemon.Current.DaemonConfig[direccion];
            ISync read = null;
            string condition = (source_con is SQLiteConnection ? "SyncGuid=?" : "SyncGuid=@SyncGuid");

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

                    schemaTable = Daemon.Current.Schema[this.CurrentPackage.TableName, direccion];
                    if (schemaTable != null)
                    {
                        switch (schemaTable.SyncDirection)
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
                        table=schemaTable.For(source_con);
                        string selection_list = table.SelectionList;
                        CommandBase command = source_con.CreateCommand($"SELECT {selection_list} FROM {table.TableName} WHERE {condition}",
                         new BaseTableQuery.Condition("SyncGuid", CurrentPackage.Guid));
                        IEnumerable<dynamic> result;
                        CompiledSetters.TryGetValue(table, out DaemonCompiledSetter compiledSetter);
                        MethodInfo method = command.GetType().GetMethod(compiledSetter is null ? "ExecuteDeferredQueryAndCompile" : nameof(CommandBase.ExecuteDeferredQuery), compiledSetter is null ? new[] { typeof(TableMapping) } : new[] { typeof(TableMapping), typeof(DaemonCompiledSetter) });
                        method = method.MakeGenericMethod(table.MappedType);
                        if (compiledSetter is null)
                        {
                            DaemonCompiledSetterTuple resultCompiled = (DaemonCompiledSetterTuple)method.Invoke(command, new object[] { table });
                            if (resultCompiled?.Results?.Any() ?? false)
                            {
                                compiledSetter = resultCompiled.CompiledSetter;
                                CompiledSetters.Add(table, compiledSetter);
                            }
                            result = resultCompiled?.Results;
                        }
                        else
                        {
                            result = (IEnumerable<dynamic>)method.Invoke(command, new object[] { table, compiledSetter });
                        }

                        if (!result?.Any() ?? false)
                        {
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (Daemon.Current.IsSleepRequested) { return false; }
                        dynamic i_result = result.First();
                        read = null;
                        if (i_result is ISync isync)
                        {
                            read = isync;
                        }
                        CanDo = ProcesarAcciones(direccion, read, target_con, source_con, action);
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
                    read?.OnSyncFailed(target_con, source_con, ex);
                    if (ex is SQLiteException)
                    {
                        if (ex.Message.Contains("no such table"))
                        {
                            target_con.CreateTable(table);
                        }
                    }
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