using Kit.Sql;
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
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Services.Interfaces;
using Kit.Daemon.Enums;
using Kit.Daemon.Sync;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Interfaces;
using Kit.Sql.Helpers;
using Kit.Sql.Reflection;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;
using TableMapping = Kit.Sql.Base.TableMapping;

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
        private static readonly Lazy<Daemon> Inicializate = new Lazy<Daemon>(Born, LazyThreadSafetyMode.ExecutionAndPublication);
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
                Raise(() => FactorDeDescanso);
                Raise(() => Inactive);
            }
        }
        public bool Inactive => (FactorDeDescanso >= DaemonConfig.MaxSleep);
        private bool IsInited;
        public Daemon SetPackageSize(int PackageSize = 25)
        {
            this.SyncManager.SetPackageSize(PackageSize);
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

        public Daemon Configure(SqlBase Local, SqlBase Remote, int DbVersion, int MaxSleep = 30)
        {
            Current.DaemonConfig = new DaemonConfig(DbVersion, Local, Remote, MaxSleep);
            Current.DaemonConfig.Local.OnConnectionStringChanged += Current.SQLH_OnConnectionStringChanged;
            Current.DaemonConfig.Remote.OnConnectionStringChanged += Current.SQLH_OnConnectionStringChanged;
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
        public Daemon SetUp(SQLServerConnection Connection)
        {
            var versionTable = Connection.Table<SyncVersions>();
            foreach (var type in new[] {
                typeof(SyncVersions), typeof(SyncDevicesInfo)
                , typeof(ChangesHistory), typeof(SyncHistory)})
            {
                TableMapping table = Connection.GetMapping(type);
                CreateTableResult result = CreateTableResult.None;
                SyncVersions version = SyncVersions.GetVersion(Connection, table);
                if (version.Version != DaemonConfig.DbVersion)
                {
                    result = Connection.CreateTable(table);
                }
                if (result != CreateTableResult.None)
                {
                    version.Version = DaemonConfig.DbVersion;
                    Connection.Update(version);
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
            Log.Logger.Information("Daemon [{0}]", "Awaking");
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

                SQLServerConnection SQLH = DaemonConfig.GetSqlServerConnection();
                CheckSyncTables(SQLH);

                if (!Device.Current.EnsureDeviceIsRegistred(SQLH))
                {
                    return;
                }

                Schema.CheckTriggers(SQLH);



                //SQLiteConnection SQLHLite = DaemonConfig.GetSqlLiteConnection();
                //SQLHLite.CreateTable<SyncHistory>();
                //IVersionControlTable controlTable = new VersionControlTable(SQLHLite);
                //if (!SQLHLite.TableExists(controlTable.TableName))
                //{
                //    controlTable.CreateTable();
                //}
                SetUp(SQLH);
                if (OnInicializate != null)
                {
                    if (!OnInicializate.Invoke())
                    {
                        return;
                    }
                }

                //Log.OnConecctionLost += Log_OnConecctionLost;
                IsInited = true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al inicializar el demonio");
            }
        }

        private void CheckSyncTables(SQLServerConnection con)
        {
            foreach (BaseTableQuery table in
                new BaseTableQuery[]{
                    con.Table<SyncVersions>(),
                    con.Table<SyncDevicesInfo>(),
                    con.Table<ChangesHistory>(),
                    con.Table<SyncHistory>()
            })
            {
                SyncVersions version = SyncVersions.GetVersion(con, table);
                if (version.Version != DaemonConfig.DbVersion)
                {
                    con.DropTable(table.Table);
                    con.CreateTable(table.Table);
                    version.Version = DaemonConfig.DbVersion;
                    version.Save(con);
                }
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
                    //lock (Locker)
                    //{
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
                                Log.Logger.Error("Daemon failed to connect");
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
                            Log.Logger.Information($"Rest :{FactorDeDescanso} mins.");
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
                        Log.Logger.Error(ex, "En start");
                    }
                    //}
                } while (true);

            }
            catch (Exception ex)
            {

                Log.Logger.Error(ex, "En la rutina Run principal");
            }
            finally
            {
                Start();
            }
        }
        public bool IsTableSynced(int id)
        {
            //if (DaemonConfig.Local is SQLiteConnection SQLHLite)
            //{
            //    bool synced = !SQLHLite.Exists($"SELECT ID FROM VERSION_CONTROL WHERE LLAVE={id} AND TABLA='R_MESAS'");
            //    if (synced)
            //    {
            //        if (DireccionActual == SyncDirecction.Local)
            //        {
            //            synced = false;
            //        }
            //    }
            //    return synced;
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}
            return true;
        }
        private bool TryToConnect()
        {
            try
            {
                Log.Logger.Debug("Daemon attemping to connect to sqlserver.");
                var conection = DaemonConfig.GetSqlServerConnection().ConnectionString;
                using (SqlConnection con = new SqlConnection(conection.ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT 1", con) { CommandType = CommandType.Text })
                    {
                        return (int)cmd.ExecuteScalar() == 1;
                    }
                }
            }
            catch (Exception ex) { Log.Logger.Verbose(ex, "Daemon attemping to connect to sqlserver."); }
            return false;
        }


        ///// <summary>
        ///// Le indica a la base de datos de sqlite que existe un nuevo registro que debe ser sincronizado
        ///// </summary>
        ///// <param name="con"></param>
        ///// <param name="TableName"></param>
        ///// <param name="PrimaryKeyValue"></param>
        ///// <param name="Accion"></param>
        public void SqliteSync(SQLiteConnection con, string TableName, object PrimaryKeyValue, AccionDemonio Accion)
        {
            //char CharAccion;
            //switch (Accion)
            //{
            //    case AccionDemonio.INSERT:
            //        CharAccion = 'I';
            //        break;
            //    case AccionDemonio.UPDATE:
            //        CharAccion = 'U';
            //        break;
            //    case AccionDemonio.DELETE:
            //        CharAccion = 'D';
            //        break;
            //    default:
            //        throw new ArgumentException("Invalid Acction", nameof(Accion));
            //}
            //if (Accion == AccionDemonio.DELETE)
            //{
            //    con.EXEC($"DELETE FROM VERSION_CONTROL WHERE TABLA=? AND LLAVE=?", TableName, PrimaryKeyValue);
            //}
            //else
            //{
            //    con.EXEC("DELETE FROM VERSION_CONTROL WHERE (ACCION='U' OR ACCION='I') AND TABLA=? AND LLAVE=?", TableName, PrimaryKeyValue);
            //}
            //con.EXEC("INSERT INTO VERSION_CONTROL(ACCION,LLAVE,TABLA) VALUES(?,?,?)", CharAccion.ToString(), PrimaryKeyValue, TableName);

        }
    }

}
