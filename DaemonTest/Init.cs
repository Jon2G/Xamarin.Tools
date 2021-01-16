using Kit;
using Kit.CadenaConexion;
using Kit.Daemon;
using Kit.Daemon.Abstractions;
using Kit.Daemon.Enums;
using Kit.Enums;
using Kit.Services;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaemonTest
{
    public class Init
    {
        public const string Version = "3.2.8";
        public Configuracion Configuracion { get; protected set; }
        public ConnectionState ConnectionState { get; protected set; }

        public static Init GetInstance()
        {
            return new Init();
        }
        protected Init()
        {
            this.ConnectionState = ConnectionState.Unknown;
        }
        public static SQLHLite SetDbPath()
        {
            SQLHelper.SQLHelper.Init(Tools.Instance.LibraryPath, Tools.Instance.Debugging);
            SQLHLite lite = new SQLHLite(Version, "Invis.db");
            //Acr.UserDialogs.UserDialogs.Instance.Alert($"Database path set to \n{lite.RutaDb}");
            return lite;
        }
        public async virtual Task<Init> Initializate()
        {
            await Task.Yield();
            SQLHLite SQLHLite = SetDbPath();
            //Check db file access
            FileInfo fileInfo = new FileInfo(SQLHLite.RutaDb);
            if (fileInfo.Exists)
            {
                if (!await FolderPermissions.Current.IsWritableReadable(fileInfo))
                {
                    await FolderPermissions.Current.TryToUnlock(fileInfo);
                }
            }

            SQLHLite
                .SetDbScriptResource<Init>("EditaLite.sql")
                .RevisarBaseDatos();

            ConfigureConnectionString(SQLHLite);
            return this;
        }
        protected virtual void ConfigureConnectionString(SQLHLite SQLHLite)
        {
            this.Configuracion = Configuracion.ObtenerConfiguracion(SQLHLite, DeviceInfo.Current.DeviceId);
            this.Configuracion = Configuracion.BuildFrom("ACUVALLE", "12345678", "1433", "192.168.0.21\\SQLEXPRESS", "sa", DeviceInfo.Current.DeviceId);
            SQLH SQLH = new SQLH(this.Configuracion.CadenaCon);
            this.Configuracion.Salvar(SQLHLite, SQLH);


            Daemon.OffLine = false;
            this.ConnectionState = TestConnectionString(SQLH);

            Daemon.Init(Kit.Services.DeviceInfo.Current)
                .Configure(Local: SQLHLite, Remote: SQLH, SQLHLite.DBVersion)
                .SetSchema(
                 new Table("PRODS", "ARTICULO", "DESCRIP", "LINEA")
                , new Table("LINEAS", "LINEA", "DESCRIP")
                , new Table("ALMACEN", "Almacen", "Descrip")
                , new Table("CLAVESADD", "clave", "articulo", "cantidad")
                , new Table("INVENTARIO_MOVIL", "ID", "GUID", "ARTICULOS_INVENTARIADOS", "LINEAS_INVENTARIADAS", "ID_ALMACEN", "ID_DISPOSITIVO")
                .ReserveNewId(true)
                //.Affects("PARTIDAS_INVENTARIO_MOVIL", "ID_INVENTARIO")
                .SetPriority(1).SetTableDirection(TableDirection.UPLOAD)
                , new Table("PARTIDAS_INVENTARIO_MOVIL", "ID", "ID_INVENTARIO", "ARTICULO", "CANTIDAD")
                .ReserveNewId(true)
                .SetPriority(2).SetTableDirection(TableDirection.UPLOAD));

            Daemon.Current.OnConnectionStateChanged += Current_OnConnectionStatusChanged;

        }



        private void Current_OnConnectionStatusChanged(object sender, EventArgs e)
        {
            //ConsoleColor background = Console.BackgroundColor;
            //ConsoleColor foreground = Console.ForegroundColor;
            Console.BackgroundColor = Daemon.OffLine ? ConsoleColor.DarkRed : ConsoleColor.Black;

        }

        protected ConnectionState TestConnectionString(SQLH SQLH)
        {
            if (string.IsNullOrEmpty(SQLH.ConnectionString))
            {
                Daemon.OffLine = true;
                return ConnectionState.NotSet;
            }

            if (Daemon.OffLine)
            {
                return ConnectionState.Unavaible;
            }
            else
            {
                Exception ex = SQLH.PruebaConexion();
                if (ex is null)
                {
                    Daemon.OffLine = false;
                    return ConnectionState.OK;
                }
                if (Log.IsDBConnectionError(ex))
                {
                    Daemon.OffLine = true;
                    return ConnectionState.Unavaible;
                }
                Daemon.OffLine = true;
            }
            return ConnectionState.Unknown;
        }


        public static Init Instance()
        {
            return new Init();
        }
    }
}
