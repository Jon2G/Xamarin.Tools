using Kit.Daemon.Abstractions;
using Kit.Daemon.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Sql.Base;
using Kit.Sql.Sqlite;
using Kit.Sql.Sync;
using SQLServer;
using static Kit.Daemon.Helpers.Helper;
using TableMapping = Kit.Sql.Base.TableMapping;
using LinqKit;

namespace Kit.Daemon.Sync
{
    public class SyncManager : ModelBase
    {

        public bool ToDo { get; set; }
        public bool NothingToDo { get => !ToDo; }
        private Queue<ChangesHistory> Pendings;
        private int TotalPendientes;
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

        private string DownloadQuery;
        private string UploadQuery;
        public SyncManager()
        {
            this.Pendings = new Queue<ChangesHistory>();
            this.Processed = 0;
            this.PackageSize = 25;
        }
        public void SetPackageSize(int PackageSize)
        {
            this.PackageSize = PackageSize;
            UploadQuery = null;
            DownloadQuery = null;
        }
        public bool Download()
        {
            if (DownloadQuery is null)
            {
                DownloadQuery = PrepareQuery(Daemon.Current.DaemonConfig[SyncDirecction.Remote]);
                Log.Logger.Information("Prepared {0} Download Query - [{1}]", "DAEMON", DownloadQuery);
            }
            return GetPendings(SyncDirecction.Local);
        }

        public bool Upload()
        {
            if (UploadQuery is null)
            {
                UploadQuery = PrepareQuery(Daemon.Current.DaemonConfig[SyncDirecction.Local]);
                Log.Logger.Information("Prepared {0} Upload Query - [{1}]", "DAEMON", UploadQuery);
            }

            return false; //return GetPendings(SyncDirecction.Remote);
        }
        private string PrepareQuery(SqlBase source)
        {
            switch (source)
            {
                case SQLServerConnection:
                    return $"select top {this.PackageSize} SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid)";
                case SQLiteConnection:
                    return $"select SyncGuid,TableName,Action from ChangesHistory limit {this.PackageSize}";
            }
            return string.Empty;
        }


        private bool GetPendings(SyncDirecction SyncTarget)
        {
            try
            {
                this.CurrentPackage = null;
                string query = string.Empty;
                switch (SyncTarget)
                {
                    case SyncDirecction.Local:
                        query = DownloadQuery;
                        break;
                    case SyncDirecction.Remote:
                        query = UploadQuery;
                        break;
                }
                var source = SyncTarget.InvertDirection();
                if (query != string.Empty)
                {
                    this.Pendings = new Queue<ChangesHistory>(Daemon.Current.DaemonConfig[source].RenewConnection()
                        .CreateCommand(query)
                        .ExecuteDeferredQuery<ChangesHistory>());
                }
                TotalPendientes = Pendings.Count;
                ToDo = TotalPendientes > 0;
                if (ToDo)
                {
                    ProcesarAcciones(SyncTarget);
                    return true;
                }

                return false;


                //using (IReader reader = Daemon.Current.DaemonConfig[SyncTarget.InvertDirection()].Read(query))
                //{
                //    if (reader is null)
                //    {
                //        return false;
                //    }
                //    if (!reader.Read())
                //    {
                //        ToDo = false;
                //        return ToDo;
                //    }
                //    else
                //    {
                //        do
                //        {
                //            try
                //            {
                //                string Accion = Convert.ToString(reader[1]);
                //                Pendientes pendiente = new Pendientes(
                //                        Accion == "I" ? AccionDemonio.INSERT :
                //                        Accion == "U" ? AccionDemonio.UPDATE :
                //                        Accion == "D" ? AccionDemonio.DELETE : AccionDemonio.INVALIDA,
                //                        reader[3], Convert.ToString(reader[2]), Convert.ToInt32(reader[0]));

                //                if (pendiente.LLave is string iave && string.IsNullOrEmpty(iave))
                //                {
                //                    pendiente.Sincronizado(SyncTarget);
                //                    continue;
                //                }
                //                if (Daemon.Current.IsSleepRequested)
                //                {
                //                    Pendings.Clear();
                //                    TotalPendientes = Pendings.Count;
                //                    ToDo = false;
                //                    return ToDo;
                //                }
                //                Pendings.Enqueue(pendiente);
                //            }
                //            catch (Exception ex)
                //            {
                //                Log.Logger.Error(ex, "Al sincronizar");
                //            }
                //        } while (reader.Read());
                //        TotalPendientes = Pendings.Count;
                //        ProcesarAcciones(SyncTarget);
                //        Pendings.Clear();
                //        ToDo = true;
                //        return ToDo;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.Logger.Error(ex, $"Obteniendo pendientes {SyncTarget}");
            }
            return false;
        }
        private void ProcesarAcciones(SyncDirecction direccion)
        {
            Processed = 0;
            CurrentPackage = null;
            SyncDirecction source = direccion.InvertDirection();
            SqlBase source_con = Daemon.Current.DaemonConfig[source];
            while (Pendings.Any())
            {
                if (Daemon.Current.IsSleepRequested)
                {
                    Pendings.Clear();
                    return;
                }
                try
                {
                    this.CurrentPackage = Pendings.Dequeue();
                    TableMapping table = Daemon.Current.Schema[this.CurrentPackage.TableName, direccion];
                    if (table != null)
                    {

                        //string key = source_con.GetTableMappingKey(this.CurrentPackage.TableName);

                        string selection_list = table.SelectionList;
                        CommandBase command = source_con.CreateCommand($"SELECT {selection_list} FROM {table.TableName} WHERE SyncGuid=@SyncGuid",
                         new BaseTableQuery.Condition("SyncGuid", CurrentPackage.SyncGuid));
                        MethodInfo method = command.GetType().GetMethod(nameof(CommandBase.ExecuteDeferredQuery), new[] { typeof(TableMapping) });
                        method = method.MakeGenericMethod(table.MappedType);
                        IEnumerable<dynamic> result = (IEnumerable<dynamic>)method.Invoke(command, new object[] { table });

                        dynamic read = result.FirstOrDefault();
                        if (read != null)
                        {
                            Daemon.Current.DaemonConfig[direccion].InsertOrReplace(read, false);
                        }

                    }
                    else
                    {
                        Log.Logger.Error("[WARNING] TABLA NO ENCONTRADA EN EL SCHEMA DEFINIDO '{0}'", this.CurrentPackage.TableName);
                        // Table.RemoveFromVersionControl(this.CurrentPackage.Tabla, Daemon.Current.DaemonConfig[direccion.InvertDirection()]);
                    }
                    CurrentPackage.MarkAsSynced(source_con);
                    Processed++;

                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        //Debugger.Break();
                    }
                    Log.Logger.Error(ex, "Al sincronizar - {0}", CurrentPackage);
                }
            }
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


    }
}
