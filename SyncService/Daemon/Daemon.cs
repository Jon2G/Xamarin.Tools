using Plugin.Xamarin.Tools.Shared.Classes;
using SQLHelper;
using SQLHelper.Abstractions;
using SQLHelper.Interfaces;
using SQLHelper.Readers;
using SQLite;
using SyncService.Daemon.Abstractions;
using SyncService.Daemon.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Log = SQLHelper.Log;

namespace SyncService.Daemon
{
    public class Daemon : ViewModelBase<Daemon>
    {
        public const string OriginName = "ORIGINDaemon";
        public const string DestinationName = "DESTINATIONDaemon";
        public DaemonConfig DaemonConfig { get; set; }
        #region ThreadMonitor
        private static object Locker = new object();
        private Thread Thread { get; set; }
        private EventWaitHandle WaitHandle { get; set; }
        #endregion
        private Table[] Schema;
        public event EventHandler OnConnectionStateChanged;

        private readonly Queue<Pendientes> Pendings;
        private int _Processed;
        private int Processed
        {
            get => _Processed;
            set
            {
                _Processed = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public static readonly string DeviceId;
        private static bool _OffLine;
        public static bool OffLine
        {
            get => _OffLine;
            set
            {
                if (_OffLine != value)
                {
                    _OffLine = value;
                    OnGlobalPropertyChanged();
                    Current.OnConnectionStateChanged?.Invoke(Current, EventArgs.Empty);
                }
            }
        }

        public static readonly Lazy<Daemon> Inicializate = new Lazy<Daemon>(Born, LazyThreadSafetyMode.ExecutionAndPublication);
        public static Daemon Current
        {
            get
            {
                Daemon ret = Inicializate.Value;
                if (ret == null)
                {
                    throw new NotSupportedException();
                }
                return ret;
            }
        }
        private bool IsAwake { get; set; } //{ get => Thread?.IsAlive ?? false; } //{ get; set; }
        private bool IsSleepRequested { get; set; }
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
        public DireccionDemonio DireccionActual
        {
            get;
            private set;
        }
        private static Daemon Born()
        {
            Daemon demon = new Daemon()
            {
                IsSleepRequested = false,
                //IsAwake = false
            };
            return demon;
        }
        private async void SQLH_OnConnectionStringChanged(object sender, EventArgs e)
        {
            await Sleep();
            Current.IsInited = false;
            Awake();
        }
        static Daemon()
        {
            DeviceId = Plugin.Xamarin.Tools.Shared.Services.DeviceInfo.Current.DeviceId;
        }
        private Daemon()
        {
            DireccionActual = DireccionDemonio.INVALID;
            IsInited = false;
            Pendings = new Queue<Pendientes>();
            Processed = 0;
            Schema = new Table[0];
        }
        public static Daemon Init(string DbVersion,int MaxSleep=30)
        {
            if (ConnectionsManager.Instance is null)
            {
                throw new InvalidOperationException("Please Init ConnectionsManager with 'ConnectionsManager.Init()' before attempting to use Daemon service");
            }

            var instance = ConnectionsManager.Instance;

            Daemon.Current.DaemonConfig = new DaemonConfig(
                DbVersion,
                instance.GetConnection(Daemon.OriginName),
                instance.GetConnection(Daemon.DestinationName), MaxSleep);

            Daemon.Current.DaemonConfig.Source.OnConnectionStringChanged += Daemon.Current.SQLH_OnConnectionStringChanged;
            Daemon.Current.DaemonConfig.Destination.OnConnectionStringChanged += Daemon.Current.SQLH_OnConnectionStringChanged;

            return Daemon.Current;
        }
        public Daemon SetSchema(params Table[] tables)
        {
            this.Schema = tables;
            return Daemon.Current;
        }
        public async void Reset()
        {
            await Sleep();
            IsInited = false;
            Awake();
        }
        private void Run()
        {
            if (Thread != null)
            {
                Awake();
                return;
            }

            WaitHandle = new AutoResetEvent(true);
            Thread = new Thread(() =>
            {
                IsAwake = true;
                Start();
                IsSleepRequested = false;
                IsAwake = false;
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            Thread.SetApartmentState(ApartmentState.STA);
            Thread.Start();
        }
        /// <summary>
        /// Despierta al demonio en caso de que este dormido,si no esta presente lo invoca,
        /// si esta ocupado le indica que busque por cambios nuevos
        /// </summary>
        public void Awake()
        {
            Log.DebugMe("DEMONIO DESPERTANDO");
            IsSleepRequested = false;
            NadaQueHacer = false;
            FactorDeDescanso = 0;
            if (WaitHandle is null)
            {
                Run();
                return;
            }
            WaitHandle.Set();
        }
        /// <summary>
        /// Duerme al demonio hasta que se vuelva a despertar 
        /// </summary>
        public async Task Sleep()
        {
            await Task.Yield();
            while (IsAwake)
            {
                IsSleepRequested = true;
                WaitHandle.Reset();
                if (Daemon.OffLine)
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            IsSleepRequested = false;
            //this.Thread = null;
            //this.WaitHandle?.Dispose();
            //this.WaitHandle = null;
        }
        private bool NadaQueHacer { get; set; }
        private int FactorDeDescanso { get; set; }
        private bool IsInited;
        private void Initialize()
        {
            try
            {
                if (!TryToConnect())
                {
                    Daemon.OffLine = true;
                }

                if (Daemon.OffLine)
                {
                    return;
                }

                foreach (SQLH SQLH in DaemonConfig.GetSqlServerConnections())
                {
                    if (!SQLH.Exists("SELECT ID_DISPOSITIVO FROM DISPOSITVOS_TABLETS WHERE ID_DISPOSITIVO=@ID_DISPOSITIVO"
                        , false, new SqlParameter("ID_DISPOSITIVO", DeviceId)))
                    {
                        SQLH.EXEC("INSERT INTO DISPOSITVOS_TABLETS(ID_DISPOSITIVO,ULTIMA_CONEXION) VALUES (@ID_DISPOSITIVO,GETDATE())"
                            , CommandType.Text, false, new SqlParameter("ID_DISPOSITIVO", DeviceId));
                    }
                    else
                    {
                        SQLH.EXEC("UPDATE DISPOSITVOS_TABLETS SET ULTIMA_CONEXION=GETDATE() WHERE ID_DISPOSITIVO=@ID_DISPOSITIVO"
                            , CommandType.Text, false, new SqlParameter("ID_DISPOSITIVO", DeviceId));
                    }

                    foreach (Table table in Schema)//this.Schema.Where(x => !x.IsTriggerChecked))
                    {
                        if (IsSleepRequested || Daemon.OffLine)
                        {
                            Start();
                            return;
                        }
                        Trigger.CheckTrigger(SQLH, table,this.DaemonConfig.DbVersion);
                    }
                }

                IsInited = true;
            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex, "Al inicializar el demonio");
            }
        }
        private void Start()
        {
            do
            {
                lock (Locker)
                {
                    try
                    {
                        if (IsSleepRequested)
                        {
                            IsAwake = false;
                            WaitHandle.WaitOne();
                        }
                        IsAwake = true;

                        if (OffLine)
                        {
                            if (!TryToConnect())
                            {
                                OffLine = true;
                                WaitHandle?.WaitOne(TimeSpan.FromSeconds(10));
                            }
                            else
                            {
                                OffLine = false;
                            }
                            NadaQueHacer = false;
                        }
                        else
                        {
                            if (!IsInited)
                            {
                                Initialize();
                                Start();
                                return;
                            }

                            try
                            {
                                //Asumir que no hay pendientes
                                NadaQueHacer = true;
                                //antes de descargar cambios subamos nuestra información que necesita ser actualizada (si existe) para evitar que se sobreescriba!
                                if (!Sync(true) && !IsSleepRequested)
                                {
                                    //actualizar los cambios pendientes en nuestra copia local (si es que hay)
                                    Sync(false);
                                }

                            }
                            catch (Exception ex)
                            {
                                Log.LogMeDemonio(ex, "Al leer datos");
                            }
                        }
                        if (NadaQueHacer)
                        {
                            FactorDeDescanso++;
                            if (FactorDeDescanso >DaemonConfig.MaxSleep)
                            {
                                FactorDeDescanso = DaemonConfig.MaxSleep;
                            }
                            //Descansar :)
                            Log.DebugMe($"Rest :{FactorDeDescanso} mins.");
                            IsAwake = false;
                            WaitHandle?.WaitOne(TimeSpan.FromSeconds(FactorDeDescanso));
                            IsAwake = true;
                        }
                        else
                        {
                            //Trabajar!!
                            FactorDeDescanso = 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.LogMeDemonio(ex, "En start");
                    }
                }
            } while (true);
        }
        public bool IsTableSynced(int id)
        {
            if (this.DaemonConfig.Source is SQLHLite SQLHLite)
            {
                bool synced = !SQLHLite.Exists($"SELECT ID FROM VERSION_CONTROL WHERE LLAVE={id} AND TABLA='R_MESAS'");
                if (synced)
                {
                    if (DireccionActual == DireccionDemonio.TO_ORIGIN)
                    {
                        synced = false;
                    }
                }
                return synced;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        private bool TryToConnect()
        {
            if (this.DaemonConfig.Source is SQLH SQLH)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(SQLH.ConnectionString))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT 1", con) { CommandType = CommandType.Text })
                        {
                            return (int)cmd.ExecuteScalar() == 1;
                        }
                    }
                }
                catch { }
                return false;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        private bool Sync(bool Upload)
        {
            try
            {
                bool Debugging = Plugin.Xamarin.Tools.Shared.Tools.Instance.Debugging;

                IReader reader = null;
                IQuery query = Select.BulidFrom(
                    Upload ? this.DaemonConfig.Destination : this.DaemonConfig.Source);
                if (query.SQLH is SQLHLite lite)
                {
                    reader = lite.Leector($@"SELECT ID,ACCION,TABLA,LLAVE FROM VERSION_CONTROL
                        ORDER BY (CASE WHEN TABLA = 'R_MESAS' 
                        THEN 1 
                        WHEN TABLA = 'R_COMANDAS' 
                        THEN 2 
                        WHEN TABLA = 'TAMANIOS_COMANDAS' 
                        THEN 3
                        WHEN TABLA = 'OPCIONES_COMANDAS' 
                        THEN 4 
                        WHEN TABLA = 'MODIFICADORES_COMANDAS' 
                        THEN 5 
                        WHEN TABLA = 'PERSONALIZADOS_COMANDAS' 
                        THEN 6 
                        WHEN TABLA = 'COMANDAS_PART' 
                        THEN 7 
                        ELSE 8 END
                        ),ID {(Debugging ? "" : "LIMIT 25")};");
                }
                else if (query.SQLH is SQLH sql)
                {
                    reader = sql.Leector(
                    $@"SELECT {(Debugging ? "" : "TOP 25")} ID,ACCION,TABLA,LLAVE FROM VERSION_CONTROL WHERE NOT EXISTS(SELECT ID_DISPOSITIVO FROM DESCARGAS_VERSIONES 
                      WHERE DESCARGAS_VERSIONES.ID_DESCARGA=VERSION_CONTROL.ID AND ID_DISPOSITIVO=@ID_DISPOSITIVO) ORDER BY TABLA DESC,LLAVE ASC;"
                      , CommandType.Text, false,
                    new SqlParameter("ID_DISPOSITIVO", DeviceId));
                }

                if (!reader.Read())
                {
                    NadaQueHacer = true;
                    return false;
                }
                else
                {
                    do
                    {
                        string Accion = Convert.ToString(reader[1]);
                        Pendings.Enqueue(
                            new Pendientes(
                                Accion == "I" ? AccionDemonio.INSERT :
                                Accion == "U" ? AccionDemonio.UPDATE :
                                Accion == "D" ? AccionDemonio.DELETE : AccionDemonio.INVALIDA,
                                reader[3], Convert.ToString(reader[2]), Convert.ToInt32(reader[0])));
                        if (IsSleepRequested)
                        {
                            NadaQueHacer = true;
                            return false;
                        }

                    } while (reader.Read());
                    TotalPendientes = Pendings.Count;
                    DireccionActual = Upload ? DireccionDemonio.TO_ORIGIN : DireccionDemonio.TO_DESTINY;
                    ProcesarAcciones(DireccionActual);
                    DireccionActual = DireccionDemonio.INVALID;

                    NadaQueHacer = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.LogMeDemonio(ex, $"Obteniendo pendientes {(Upload ? DireccionDemonio.TO_ORIGIN : DireccionDemonio.TO_DESTINY)}");
            }
            return false;
        }
        private void ProcesarAcciones(DireccionDemonio direccion)
        {
            Processed = 0;

            while (Pendings.Any())
            {
                if (IsSleepRequested)
                {
                    Pendings.Clear();
                    return;
                }
                try
                {
                    Pendientes pendiente = Pendings.Dequeue();
                    Table table = Schema.First(x => x.Name == pendiente.Tabla);
                    if (!table.Execute(this.DaemonConfig, pendiente, direccion))
                    {
                        if (direccion == DireccionDemonio.TO_ORIGIN)//deben ir en un orden especifico
                        {
                            Processed = 0;
                            return;
                        }
                    }
                    Processed++;
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    Log.LogMeDemonio(ex, "Al sincronizar");
                }
            }
        }

        internal void SqliteSync(SQLiteConnection con, string TableName, object PrimaryKeyValue, AccionDemonio Accion)
        {
            using (SQLHelper.SQLHLite SQLHLite = new SQLHLite(new FileInfo(con.DatabasePath)))
            {
                if (con.Handle?.IsClosed ?? true)
                {
                    con.Dispose();
                    con = SQLHLite.Conecction();
                }
                char CharAccion;
                switch (Accion)
                {
                    case AccionDemonio.INSERT:
                        CharAccion = 'I';
                        break;
                    case AccionDemonio.UPDATE:
                        CharAccion = 'U';
                        break;
                    case AccionDemonio.DELETE:
                        CharAccion = 'D';
                        break;
                    default:
                        throw new ArgumentException("Invalid Acction", nameof(Accion));
                }
                if (Accion == AccionDemonio.DELETE)
                {
                    SQLHLite.EXEC(con, "DELETE FROM VERSION_CONTROL WHERE TABLA=? AND LLAVE=?", TableName, PrimaryKeyValue);
                }
                else
                {
                    SQLHLite.EXEC(con, "DELETE FROM VERSION_CONTROL WHERE (ACCION='U' OR ACCION='I') AND TABLA=? AND LLAVE=?", TableName, PrimaryKeyValue);
                }
                SQLHLite.EXEC(con, "INSERT INTO VERSION_CONTROL(ACCION,LLAVE,TABLA) VALUES(?,?,?)", CharAccion.ToString(), PrimaryKeyValue, TableName);
                con.Close();
            }
        }
    }

}
