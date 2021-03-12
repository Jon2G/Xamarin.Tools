using Kit.Daemon.Abstractions;
using Kit.Daemon.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Model;
using Kit.Sql.Base;
using Kit.Sql.Exceptions;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;
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
            this.PackageSize = 50;
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
            return GetPendings(SyncDirecction.Remote);
        }
        private string PrepareQuery(SqlBase source)
        {
            switch (source)
            {
                case SQLServerConnection:
                    string query = "SELECT ";
                    if (PackageSize > 0)
                    {
                        query += $"TOP {this.PackageSize}";
                    }
                    query += $" SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid)";
                    return query;
                case SQLiteConnection:
                    return $"select SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid) limit {this.PackageSize}";
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
            TableMapping table = null;
            SyncDirecction source = direccion.InvertDirection();
            SqlBase source_con = Daemon.Current.DaemonConfig[source];
            SqlBase target_con = Daemon.Current.DaemonConfig[direccion];
            string condition = (source_con is SQLiteConnection ? "SyncGuid=?" : "SyncGuid=@SyncGuid");
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
                    table = Daemon.Current.Schema[this.CurrentPackage.TableName, direccion];
                    if (table != null)
                    {

                        //string key = source_con.GetTableMappingKey(this.CurrentPackage.TableName);

                        string selection_list = table.SelectionList;
                        CommandBase command = source_con.CreateCommand($"SELECT {selection_list} FROM {table.TableName} WHERE {condition}",
                         new BaseTableQuery.Condition("SyncGuid", CurrentPackage.SyncGuid));
                        MethodInfo method = command.GetType().GetMethod(nameof(CommandBase.ExecuteDeferredQuery), new[] { typeof(TableMapping) });
                        method = method.MakeGenericMethod(table.MappedType);
                        IEnumerable<dynamic> result = (IEnumerable<dynamic>)method.Invoke(command, new object[] { table });

                        dynamic read = result.FirstOrDefault();
                        if (read != null)
                        {
                            object old_pk = null;
                            //if (table.HasAutoIncPK && table.SyncMode.ReserveNewId)
                            //{
                            //    Type readType = read.GetType();
                            //    PropertyInfo pkProperty = readType.GetProperty(table.PK.PropertyName);
                            //    switch (pkProperty.GetValue(read))
                            //    {
                            //        case int _:
                            //            pkProperty.SetValue(read, 0);
                            //            break;
                            //        default:
                            //            throw new InvalidDataException("No default value for empty pk");
                            //    }
                            //}
                            Type readType = read.GetType();
                            if (table.PK!=null)
                            {
                                PropertyInfo pkProperty = readType.GetProperty(table.PK.PropertyName);
                                old_pk = pkProperty.GetValue(read);
                            }

                            if (!string.IsNullOrEmpty(table.SyncMode.CustomUploadAction) && target_con is SQLServerConnection)
                            {
                                object affects_result =
                                    readType.GetMethod(table.SyncMode.CustomUploadAction).Invoke(read, new object[] { source_con, (SqlBase)target_con });
                                if (affects_result is bool b && b)
                                {
                                    CurrentPackage.MarkAsSynced(source_con);
                                    Processed++;
                                    return;
                                }
                            }


                            if (target_con is SQLiteConnection)
                            {
                                target_con.InsertOrReplace(read, false);
                            }
                            else
                            {
                                target_con.Table<ChangesHistory>().Delete(x => x.SyncGuid == CurrentPackage.SyncGuid);
                                target_con.Insert(read, String.Empty, read.GetType(), false);
                            }
                            if (table.HasAutoIncPK)
                            {
                                if (!string.IsNullOrEmpty(table.SyncMode.AffectsMethodName) && target_con is SQLServerConnection)
                                {
                                    object affects_result =
                                        readType.GetMethod(table.SyncMode.AffectsMethodName).Invoke(read, new object[] { source_con, old_pk });
                                }
                            }
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
