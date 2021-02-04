using Kit.Sql;
using SQLite;
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
using Kit.Daemon.Abstractions;
using Kit.Daemon.VersionControl;
using Kit.Enums;
using Kit.Services.Interfaces;
using Kit.Daemon.Enums;
using Kit.Daemon.Sync;
using Kit.Model;
using Kit.Sql.Interfaces;
using Kit.Sql.Helpers;
using Kit.Sql.Reflection;

namespace Kit.Daemon
{
    [Preserve(AllMembers = true)]
    public class Daemon : StaticModel<Daemon>
    {
        public DaemonConfig DaemonConfig { get; set; }
        #region ThreadMonitor
        private static object Locker = new object();
        private Thread Thread { get; set; }
        private EventWaitHandle WaitHandle { get; set; }
        #endregion
        public Schema Schema { get; private set; }
        public event EventHandler OnConnectionStateChanged;
        public event Func<bool> OnInicializate;
        public static string DeviceId { get; private set; }
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
        internal bool IsSleepRequested { get; set; }
        private SyncManager _SyncManager;
        public SyncManager SyncManager
        {
            get => _SyncManager;
            set
            {
                _SyncManager = value;
                Raise(() => SyncManager);
            }
        }

        private int _FactorDeDescanso;
        public int FactorDeDescanso
        {
            get => _FactorDeDescanso;
            private set
            {
                _FactorDeDescanso = value;
                Raise(()=>FactorDeDescanso);
                Raise(()=>Inactive);
            }
        }
        public bool Inactive => (FactorDeDescanso >= DaemonConfig.MaxSleep);
        private bool IsInited;
        public Daemon SetPackageSize(int PackageSize = 25)
        {
            this.SyncManager.PackageSize = PackageSize;
            this.SyncManager.ReGenerateSyncQueries();
            return this;
        }
        public SyncDirecction DireccionActual
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
        private Daemon()
        {
            this.DireccionActual = SyncDirecction.INVALID;
            this.IsInited = false;
            this.SyncManager = new SyncManager();
            this.Schema = new Schema();
        }
        public static Daemon Init(IDeviceInfo DeviceInfo)
        {
            Daemon.DeviceId = DeviceInfo.DeviceId;
            return Current;
        }
        public Daemon Configure(BaseSQLHelper Local, BaseSQLHelper Remote, ulong DbVersion, int MaxSleep = 30)
        {
            Current.DaemonConfig = new DaemonConfig(DbVersion, Local, Remote, MaxSleep);
            Current.DaemonConfig.Local.OnConnectionStringChanged += Current.SQLH_OnConnectionStringChanged;
            Current.DaemonConfig.Remote.OnConnectionStringChanged += Current.SQLH_OnConnectionStringChanged;
            return this;
        }
        public Daemon SetSchema(params Table[] tables)
        {
            this.Schema = new Schema(tables);
            this.SyncManager.ReGenerateSyncQueries();
            return Current;
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
        /// Comprueba que las tablas de versión coincidan con la versión de este servicio, las crea ó renueva según sea el caso
        /// </summary>
        public Daemon SetUp(SqlServer Connection)
        {
            using (ReflectionCaller reflex = new Kit.Sql.Reflection.ReflectionCaller())
            {
                foreach (IVersionControlTable table in reflex.GetInheritedClasses<IVersionControlTable>(Connection).OrderBy(x => x.Priority))
                {
                    if (table.GetVersion() != DaemonConfig.DbVersion)
                    {
                        table.DropTable();
                        if (!Connection.TableExists(table.TableName))
                        {
                            table.CreateTable();
                        }
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Despierta al demonio en caso de que este dormido,si no esta presente lo invoca,
        /// si esta ocupado le indica que busque por cambios nuevos
        /// </summary>
        public void Awake()
        {
            Log.DebugMe("DEMONIO DESPERTANDO");
            IsSleepRequested = false;
            this.SyncManager.ToDo = true;
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
        public async Task<Daemon> Sleep()
        {
            await Task.Yield();
            while (IsAwake)
            {
                IsSleepRequested = true;
                WaitHandle.Reset();
                if (OffLine)
                {
                    return this;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }
            IsSleepRequested = false;
            //this.Thread = null;
            //this.WaitHandle?.Dispose();
            //this.WaitHandle = null;
            return this;
        }

        private void Initialize()
        {
            try
            {
                if (!TryToConnect())
                {
                    OffLine = true;
                }

                if (OffLine)
                {
                    return;
                }
                SqlServer SQLH = DaemonConfig.GetSqlServerConnection();
                ///SQL SERVER
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

                foreach (Table table in Schema.DownloadTables)
                {
                    if (IsSleepRequested || OffLine)
                    {
                        Start();
                        return;
                    }
                    Trigger.CheckTrigger(SQLH, table, DaemonConfig.DbVersion);
                }


                SqLite SQLHLite = DaemonConfig.GetSqlLiteConnection();
                IVersionControlTable controlTable = new VersionControlTable(SQLHLite);
                if (!SQLHLite.TableExists(controlTable.TableName))
                {
                    controlTable.CreateTable();
                }

                SetUp(DaemonConfig.GetSqlServerConnection());
                if (OnInicializate != null)
                {
                    if (!OnInicializate.Invoke())
                    {
                        return;
                    }
                }
                Log.OnConecctionLost += Log_OnConecctionLost;
                IsInited = true;
            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex, "Al inicializar el demonio");
            }
        }

        private void Log_OnConecctionLost(object sender, EventArgs e)
        {
            Daemon.OffLine = true;
        }

        private void Start()
        {
            try
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
                                this.SyncManager.ToDo = true;
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
                                    this.SyncManager.ToDo = false;
                                    //antes de descargar cambios subamos nuestra información que necesita ser actualizada (si existe) para evitar que se sobreescriba!
                                    if (!this.SyncManager.Upload() && !IsSleepRequested)
                                    {
                                        //actualizar los cambios pendientes en nuestra copia local (si es que hay)
                                        this.SyncManager.Download();
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Log.LogMeDemonio(ex, "Al leer datos");
                                }
                            }
                            if (this.SyncManager.NothingToDo)
                            {
                                FactorDeDescanso++;
                                if (FactorDeDescanso > DaemonConfig.MaxSleep)
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
            catch (Exception ex)
            {

                Log.LogMeDemonio(ex, "En la rutina Run principal");
            }
            finally
            {
                Start();
            }
        }
        public bool IsTableSynced(int id)
        {
            if (DaemonConfig.Local is SqLite SQLHLite)
            {
                bool synced = !SQLHLite.Exists($"SELECT ID FROM VERSION_CONTROL WHERE LLAVE={id} AND TABLA='R_MESAS'");
                if (synced)
                {
                    if (DireccionActual == SyncDirecction.Local)
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
            try
            {
                string conection = DaemonConfig.GetSqlServerConnection().ConnectionString;
                using (SqlConnection con = new SqlConnection(conection))
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
        /// <summary>
        /// Le indica a la base de datos de sqlite que existe un nuevo registro que debe ser sincronizado
        /// </summary>
        /// <param name="con"></param>
        /// <param name="TableName"></param>
        /// <param name="PrimaryKeyValue"></param>
        /// <param name="Accion"></param>
        public void SqliteSync(SqLite con, string TableName, object PrimaryKeyValue, AccionDemonio Accion)
        {
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
                con.EXEC($"DELETE FROM VERSION_CONTROL WHERE TABLA=? AND LLAVE=?", TableName, PrimaryKeyValue);
            }
            else
            {
                con.EXEC("DELETE FROM VERSION_CONTROL WHERE (ACCION='U' OR ACCION='I') AND TABLA=? AND LLAVE=?", TableName, PrimaryKeyValue);
            }
            con.EXEC("INSERT INTO VERSION_CONTROL(ACCION,LLAVE,TABLA) VALUES(?,?,?)", CharAccion.ToString(), PrimaryKeyValue, TableName);

        }
    }

}
