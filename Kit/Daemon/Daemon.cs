using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kit.Daemon.Abstractions;
using Kit.Daemon.Devices;
using Kit.Daemon.Enums;
using Kit.Daemon.Sync;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Tables;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using Kit.Entity;
using Kit.Enums;
using Kit.Sql.SqlServer;
using ThreadState = System.Diagnostics.ThreadState;
using System.Data.Entity;

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

        #endregion ThreadMonitor

        public Schema Schema { get; private set; }
        public ICommand OnConnectionStateChanged;

        public event Func<bool> OnInicializate;

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
                    Current.OnPropertyChanged(nameof(ISOffline));
                    Current.OnConnectionStateChanged?.Execute(Current);
                }
            }
        }

        public bool ISOffline => OffLine;

        private static Lazy<Daemon> Inicializate = new Lazy<Daemon>(Born, LazyThreadSafetyMode.ExecutionAndPublication);

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

        internal bool IsAwake
        {
            get;
            set;
        }

        //{ get => Thread?.IsAlive ?? false; } //{ get; set; }
        internal bool IsSleepRequested
        {
            get;
            private set;
        }

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
                if (_FactorDeDescanso != value)
                {
                    _FactorDeDescanso = value;
                    Raise(() => FactorDeDescanso);
                    Raise(() => Inactive);
                }
            }
        }

        public bool Inactive => !IsAwake && (FactorDeDescanso >= DaemonConfig.MaxSleep);
        public bool IsInited { get; private set; }

        public Daemon SetPackageSize(int PackageSize = Sync.SyncManager.RegularPackageSize)
        {
            this.SyncManager.SetPackageSize(PackageSize);
            return this;
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
            this.IsInited = false;
            this.SyncManager = new SyncManager();
            this.Schema = new Schema();
        }

        public Daemon Configure(IDbConnection Local, IDbConnection Remote, int DbVersion, int MaxSleep = 30)
        {
            Current.DaemonConfig = new DaemonConfig(DbVersion, Local, Remote, MaxSleep);
            //Current.DaemonConfig.Local.OnConnectionStringChanged += Current.SQLH_OnConnectionStringChanged;
            //Current.DaemonConfig.Remote.OnConnectionStringChanged += Current.SQLH_OnConnectionStringChanged;
            Log.Logger.Debug("Daemon has been configured");
            return this;
        }

        public Daemon SetSchema(params Type[] tables)
        {
            this.Schema = new Schema(tables);
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

            WaitHandle = new ManualResetEvent(false); //new AutoResetEvent(true);
            Thread = new Thread(() =>
            {
                IsAwake = true;
                bool result = Start();
                IsSleepRequested = false;
                IsAwake = false;
                WaitHandle.Dispose();
                WaitHandle = null;
                Log.Logger.Information("Daemon thread has been finished , result {0}", result);
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            Thread.TrySetApartmentState(ApartmentState.STA);
            Thread.Start();
        }

        /// <summary>
        /// Despierta al demonio en caso de que este dormido,si no esta presente lo invoca,
        /// si esta ocupado le indica que busque por cambios nuevos
        /// </summary>
        public async Task AwakeAsync()
        {
            await Task.Yield();
            Awake();
        }

        private void Awake()
        {
            if (Tools.Debugging)
                Log.Logger.Information("Daemon [{0}]", "Awaking");
            IsSleepRequested = false;
            this.SyncManager.ToDo = true;
            FactorDeDescanso = 0;
            if (IsAwake)
            {
                return;
            }
            if (Thread is not null)
                switch (Thread.ThreadState)
                {
                    case System.Threading.ThreadState.Aborted:
                    case System.Threading.ThreadState.AbortRequested:
                    case System.Threading.ThreadState.StopRequested:
                    case System.Threading.ThreadState.Stopped:
                    case System.Threading.ThreadState.Unstarted:
                        WaitHandle?.Dispose();
                        WaitHandle = null;
                        Thread = null;
                        break;
                    case System.Threading.ThreadState.Background | System.Threading.ThreadState.WaitSleepJoin:
                    case System.Threading.ThreadState.WaitSleepJoin:
                        WaitHandle.Set();
                        Awake();
                        return;

                }
            if (WaitHandle is null)
            {
                Run();
                return;
            }
            WaitHandle.Reset();
            WaitHandle.Set();
            while (!IsAwake)
            {
                Awake();
            }
        }

        public async Task Destroy()
        {
            await Sleep();
            WaitHandle.Close();
            WaitHandle.Dispose();
            //Thread.Abort();
            Thread = null;
            WaitHandle = null;
            Inicializate = new Lazy<Daemon>(Born, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Duerme al demonio hasta que se vuelva a despertar
        /// </summary>
        /// </summary>
        public async Task<Daemon> Sleep()
        {
            Log.Logger.Debug("DAEMON COMMANDED TO SLEEP ACTUALLY IS =>{0}", this.IsAwake);
            IsSleepRequested = true;
            await Task.Yield();
            while (IsAwake)
            {
                WaitHandle.Reset();
                if (OffLine)
                {
                    return this;
                }
                await Task.Delay(10);
            }
            //this.Thread = null;
            //this.WaitHandle?.Dispose();
            //this.WaitHandle = null;
            return this;
        }

        private void Initialize()
        {
            try
            {
                Log.Logger.Debug("Attemping to Initialize Daemon");
                if (!TryToConnect())
                {
                    OffLine = true;
                    Log.Logger.Error("Attemping to Initialize Daemon - {0}", "FAILED");
                }

                if (OffLine)
                {
                    return;
                }

                SqlConnection SQLH = DaemonConfig.GetSqlServerConnection() as SqlConnection;
                SqlServerInformation version = (SQLH as SqlConnection).GetServerInformation();
                if (version.Version > SqlServerVersion.V2016)
                {
                    SQLH.SetCacheIdentity(false);
                }
                if (!Device.Current.EnsureDeviceIsRegistred(SQLH))
                {
                    return;
                }
                DaemonConfig.Remote.GetContext().CheckTables(Schema.AllTables.ToArray());
                DaemonConfig.Local.GetContext().CheckTables(Schema.DownloadTables.ToArray());
                Schema.CheckTriggers(SQLH);
                if (OnInicializate != null)
                {
                    if (!OnInicializate.Invoke())
                    {
                        return;
                    }
                }

                DeviceInformation device = DaemonConfig.Local.Table<DeviceInformation>().FirstOrDefault() ??
                                           new DeviceInformation()
                                           {
                                               IsFirstLaunchTime = true,
                                               DeviceId = Device.Current.DeviceId
                                           };
                if (device.IsFirstLaunchTime)
                {
                    device.IsFirstLaunchTime = false;
                    //I Have been deleted and reinstalled! , so i need to sync everything again...

                    var table = DaemonConfig.Remote.Table<SyncHistory>();
                    table.RemoveRange(table.Where(x => x.DeviceId == device.DeviceId));
                    DaemonConfig.Local.Update(device);
                }
                IsInited = true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al inicializar el demonio");
            }
        }

        private void Log_OnConecctionLost(object sender, EventArgs e)
        {
            Daemon.OffLine = true;
        }

        private bool Start()
        {
            try
            {
                do
                {
                    //lock (Locker)
                    //{
                    try
                    {
                        if (WaitHandle is null)
                        {
                            return false;
                        }
                        if (IsSleepRequested && !OffLine)
                        {
                            GotoSleep();
                        }
                        IsAwake = true;

                        if (OffLine)
                        {
                            if (!TryToConnect())
                            {
                                IsAwake = false;
                                OffLine = true;
                                Log.Logger.Error("Daemon failed to connect");
                                WaitHandle?.WaitOne(TimeSpan.FromSeconds(10));
                                WaitHandle.Reset();
                            }
                            else
                            {
                                WaitHandle.Set();
                                IsAwake = true;
                                IsSleepRequested = false;
                                OffLine = false;
                                return Start();
                            }
                            this.SyncManager.ToDo = true;
                        }
                        else
                        {
                            if (!IsInited)
                            {
                                Initialize();
                                return Start();
                            }

                            try
                            {
                                //Asumir que no hay pendientes
                                this.SyncManager.ToDo = false;
                                this.IsAwake = true;

                                //antes de descargar cambios subamos nuestra información que necesita ser actualizada (si existe) para evitar que se sobreescriba!
                                if (!this.SyncManager.Upload() && !IsSleepRequested)
                                {
                                    this.IsAwake = true;
                                    //actualizar los cambios pendientes en nuestra copia local (si es que hay)
                                    if (!this.SyncManager.Download())
                                    {
                                        this.SyncManager.CurrentDirection = SyncTarget.NOT_SET;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Logger.Error(ex, "Al leer datos");
                            }
                        }
                        if (this.SyncManager.NothingToDo)
                        {
                            FactorDeDescanso++;
                            //if (FactorDeDescanso > DaemonConfig.MaxSleep)
                            //{
                            //    FactorDeDescanso = DaemonConfig.MaxSleep;
                            //}
                            //Descansar :)
                            Log.Logger.Information($"Rest :{FactorDeDescanso} seconds.");
                            IsAwake = false;
                            Thread.Sleep(TimeSpan.FromSeconds(FactorDeDescanso));
                            //GotoSleep();
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
                        Log.Logger.Error(ex, "En start");
                    }
                    //}
                } while (true);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "En la rutina Run principal");
                return Start();
            }
        }

        private void GotoSleep(bool HasBeenForcedToSleep = false)
        {
            IsAwake = false;
            bool signaled = WaitHandle.WaitOne();
            if (signaled && !HasBeenForcedToSleep)
            {
                HasBeenForcedToSleep = true;
                WaitHandle.Reset();
                GotoSleep(HasBeenForcedToSleep);
                return;
            }
            IsAwake = true;
            IsSleepRequested = false;
        }

        private bool TryToConnect()
        {
            Log.Logger.Debug("Daemon attemping to connect to sqlserver.");
            return DaemonConfig.Remote.TryToConnect(out _);
        }

        ///// <summary>
        ///// Le indica a la base de datos de sqlite que existe un nuevo registro que debe ser sincronizado
        ///// </summary>
        ///// <param name="con"></param>
        ///// <param name="TableName"></param>
        ///// <param name="PrimaryKeyValue"></param>
        ///// <param name="Accion"></param>
        public void SqliteSync(IDbConnection con, string TableName, Guid SyncGuid, NotifyTableChangedAction Accion, int Priority)
        {
            //   con.UpdateVersionControl(new ChangesHistory(TableName, SyncGuid, Accion, Priority));
        }
    }
}