using Kit.Daemon.Abstractions;
using Kit.Daemon.Enums;
using Kit.Sql;
using Kit.Sql.Abstractions;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kit.Daemon.Devices;
using static Kit.Daemon.Helpers.Helper;

namespace Kit.Daemon.Sync
{
    public class SyncManager : ModelBase
    {
        private string DownloadQuery { get; set; }
        private string UploadQuery { get; set; }
        public bool ToDo { get; set; }
        public bool NothingToDo { get => !ToDo; }
        private readonly Queue<Pendientes> Pendings;
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
        public int PackageSize { get; internal set; }
        private Pendientes _CurrentPackage;
        public Pendientes CurrentPackage
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
            this.Pendings = new Queue<Pendientes>();
            this.Processed = 0;
            this.PackageSize = 25;
        }
        public bool Download()
        {
            if (string.IsNullOrEmpty(this.DownloadQuery))
            {
                return false;
            }
            return GetPendings(SyncDirecction.Local, this.DownloadQuery);
        }
        public bool Upload()
        {
            if (string.IsNullOrEmpty(this.UploadQuery))
            {
                return false;
            }
            return GetPendings(SyncDirecction.Remote, this.UploadQuery);
        }

        private bool GetPendings(SyncDirecction SyncTarget, string query)
        {
            try
            {
                using (IReader reader = Daemon.Current.DaemonConfig[SyncTarget.InvertDirection()].Read(query))
                {
                    if (reader is null)
                    {
                        return false;
                    }
                    if (!reader.Read())
                    {
                        ToDo = false;
                        return ToDo;
                    }
                    else
                    {
                        do
                        {
                            try
                            {
                                string Accion = Convert.ToString(reader[1]);
                                Pendientes pendiente = new Pendientes(
                                        Accion == "I" ? AccionDemonio.INSERT :
                                        Accion == "U" ? AccionDemonio.UPDATE :
                                        Accion == "D" ? AccionDemonio.DELETE : AccionDemonio.INVALIDA,
                                        reader[3], Convert.ToString(reader[2]), Convert.ToInt32(reader[0]));

                                if (pendiente.LLave is string iave && string.IsNullOrEmpty(iave))
                                {
                                    pendiente.Sincronizado(SyncTarget);
                                    continue;
                                }
                                if (Daemon.Current.IsSleepRequested)
                                {
                                    Pendings.Clear();
                                    TotalPendientes = Pendings.Count;
                                    ToDo = false;
                                    return ToDo;
                                }
                                Pendings.Enqueue(pendiente);
                            }
                            catch (Exception ex)
                            {
                                Log.LogMe(ex, "Al sincronizar");
                            }
                        } while (reader.Read());
                        TotalPendientes = Pendings.Count;
                        ProcesarAcciones(SyncTarget);
                        Pendings.Clear();
                        ToDo = true;
                        return ToDo;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.LogMeDemonio(ex, $"Obteniendo pendientes {SyncTarget}");
            }
            return false;
        }
        private void ProcesarAcciones(SyncDirecction direccion)
        {
            Processed = 0;
            CurrentPackage = null;
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
                    Table table = Daemon.Current.Schema[this.CurrentPackage.Tabla, direccion];
                    if (table != null)
                    {
                        if (!table.Execute(this.CurrentPackage, direccion))
                        {
                            if (direccion == SyncDirecction.Local)//deben ir en un orden especifico
                            {
                                Processed = 0;
                                return;
                            }
                        }
                    }
                    else
                    {
                        Log.LogMe($"[WARNING] TABLA NO ENCONTRADA EN EL SCHEMA DEFINIDO '{this.CurrentPackage.Tabla}'");
                        Table.RemoveFromVersionControl(this.CurrentPackage.Tabla, Daemon.Current.DaemonConfig[direccion.InvertDirection()]);
                    }
                    Processed++;
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        //Debugger.Break();
                    }
                    Log.LogMeDemonio(ex, "Al sincronizar");
                }
            }
        }
        internal void ReGenerateSyncQueries()
        {
            GenerateSyncQuery(SyncDirecction.Local);
            GenerateSyncQuery(SyncDirecction.Remote);
        }
        public void GenerateSyncQuery(SyncDirecction dirreccion)
        {
            BaseSQLHelper sql = Daemon.Current.DaemonConfig[dirreccion]; //.Destination
            string query = null;
            if (sql is SqLite)
            {
                query = BuildSqliteSelectQuery();
            }
            else if (sql is SqlServer)
            {
                query = BuildSqlServerQuery();
            }

            switch (dirreccion.InvertDirection())
            {
                case SyncDirecction.Remote:
                    this.UploadQuery = query;
                    break;
                case SyncDirecction.Local:
                    this.DownloadQuery = query;
                    break;
            }
        }
        private string BuildSqliteSelectQuery()
        {
            Schema schema = Daemon.Current.Schema;
            if (Daemon.Current.Schema.HasUploadTables)
            {
                StringBuilder builder = new StringBuilder().Append("SELECT ID,ACCION,TABLA,LLAVE FROM VERSION_CONTROL ORDER BY(CASE ");
                foreach (Table table in schema.UploadTables)
                {
                    builder.Append(" WHEN TABLA = '")
                                        .Append(table.Name)
                                        .Append("' THEN ").Append(table.Priority);
                }
                builder.Append(" ELSE ").Append(schema.UploadTables.Last().Priority + 1)
                    .Append(" END),ID").Append(PackageSize <= 0 ? ";" : $" LIMIT {PackageSize};");
                return builder.ToString();
            }
            return null;
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
