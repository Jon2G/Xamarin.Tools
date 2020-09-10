using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQLHelper
{
    public static class Log
    {
        public static event EventHandler OnConecctionLost;
        public static string LogDirectory { get; private set; }
        public static string LogPath { get; private set; }
        public static string BackgroundLogPath { get; private set; }
        public static string DBLogPath { get; private set; }
        public static string CriticalLogPath { get; private set; }
        private static EventHandler OnAlertCritical;
        public static void Init(string LogDirectory, EventHandler CriticalAction = null)
        {
            Log.LogDirectory = LogDirectory;
            Log.CriticalLogPath = $"{Log.LogDirectory}\\Critical.log";
            Log.LogPath = $"{Log.LogDirectory}\\AppData.log";
            Log.BackgroundLogPath = $"{Log.LogDirectory}\\AppDataDemonio.log";
            Log.DBLogPath = $"{Log.LogDirectory}\\SQL_T.sql";
            if (!Directory.Exists(Log.LogDirectory))
            {
                Directory.CreateDirectory(Log.LogDirectory);
            }
            if (CriticalAction != null)
            {
                OnAlertCritical += CriticalAction;
                AlertCriticalUnhandled();
            }
        }
        private static void AlertCriticalUnhandled()
        {
            FileInfo file = new FileInfo(Log.CriticalLogPath);
            if (file.Exists)
            {
                string criticalDescription = File.ReadAllText(file.FullName);
                file.Delete();
                if (!string.IsNullOrEmpty(criticalDescription))
                {
                    OnAlertCritical?.Invoke(criticalDescription, EventArgs.Empty);
                }
            }
        }
        public static void ChangeLogPath(string Path)
        {
            Log.BackgroundLogPath = $"{Path}\\AppDataDemonio.log";
            Log.LogPath = $"{Path}\\AppData.log";
            Log.DBLogPath = $"{Path}\\SQL_T.sql";
            Log.LogDirectory = Path;
            if (!Directory.Exists(Log.LogDirectory))
            {
                Directory.CreateDirectory(Log.LogDirectory);
            }
        }
        /// <summary>
        /// Retorna la excepcion base de donde se origino el error
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception MainExcepcion(this Exception ex)
        {
            Exception Exbase = ex;
            while (Exbase?.InnerException != null)
            {
                Exbase = Exbase.InnerException;
            }
            return Exbase;
        }
        public static void DebugMe(string text)
        {
            if (SQLHelper.Instance?.Debugging ?? false)
            {
                LogMe(text);
            }
        }
        public static void LogMe(string error)
        {
            try
            {
                string mensaje = string.Concat("\r", Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "---->", error);
                if (SQLHelper.Instance.Debugging)
                {
                    Debug.WriteLine(mensaje);
                    Console.Write(mensaje);
                    return;
                }
                File.AppendAllText(Log.LogPath, mensaje);
            }
            catch (Exception)
            {
            }
        }
        public static void LogMe(Exception error)
        {
            if (error?.GetType()?.Name == "SDKException")
            {
                //ignorar errores del lector n.n
                return;
            }
            try
            {
                string mensaje = string.Concat(Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "----> ", error.Message);
                if (SQLHelper.Instance.Debugging)
                {
                    LogMe(mensaje);
                    return;
                }
                File.AppendAllText(Log.LogPath, mensaje);
            }
            catch (Exception)
            {
            }
        }
        public static StackFrame MainStackFrame(Exception exception)
        {
            StackTrace st = new StackTrace(exception, true);
            if (st is null)
            {
                return null;
            }
            if (st.FrameCount > 0)
            {
                List<StackFrame> frames = st.GetFrames().ToList();
                if (frames?.Count <= 0)
                {
                    return null;
                }
                return frames.Last();
            }
            return null;
        }

        public static void LogCritical(Exception ex)
        {
            LogUnhandledException(ex);
        }

        public static void LogMe(Exception error, string descripcion, bool StackTrace = false)
        {
            try
            {
                string mensaje = string.Concat(Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString(), descripcion, "----> ", error.Message);
                if (StackTrace)
                {
                    StackFrame frame = MainStackFrame(error);
                    LogMe("STACK FRAME\n L:" + frame?.GetFileLineNumber() ?? -1 + (" Frame:" + frame ?? "Desconocido") + "\n");
                }
                if (SQLHelper.Instance.Debugging)
                {
                    Console.Write(mensaje);
                    return;
                }
                File.AppendAllText(Log.LogPath, mensaje);
            }
            catch (Exception)
            {
                //Ignorar las excepciones del logueo
            }
            if (Debugger.IsAttached)
            {
                throw error;
            }
        }
        public static void LogMeDemonio(Exception error, string descripcion)
        {
            try
            {
                string mensaje = string.Concat(Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString(), descripcion, "----> ", error.Message);
                if (Log.IsDBConnectionError(error))
                {
                    Log.OnConecctionLost?.Invoke(error, EventArgs.Empty);
                }
                if (SQLHelper.Instance.Debugging)
                {
                    Log.LogMe(mensaje);
                    return;
                }
                File.AppendAllText(Log.BackgroundLogPath, mensaje);

            }
            catch (Exception)
            {
            }
        }
        public static void LogMeDemonio(string error)
        {
            try
            {
                string mensaje = string.Concat(Environment.NewLine, "[WARNING]",
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "----> ", error);
                if (SQLHelper.Instance.Debugging)
                {
                    Console.Write(mensaje);
                    return;
                }
                File.AppendAllText(Log.BackgroundLogPath, mensaje);
            }
            catch (Exception)
            {
            }
        }
        public static void RenovarLogs()
        {
            Thread th = new Thread(() =>
            {
                try
                {
                    string DirectorioDesechos = $"{Log.LogDirectory}\\DisposedLogs";
                    foreach (string l in new string[] { Log.LogPath, Log.BackgroundLogPath, Log.DBLogPath })
                    {
                        FileInfo log = new FileInfo(l);
                        if (log.Exists && DateTime.Now.Subtract(log.CreationTime).Days > 15
                        ) //Cada 15 dias renueva el log
                        {
                            DirectoryInfo dir = new DirectoryInfo(DirectorioDesechos);
                            if (!dir.Exists)
                            {
                                dir.Create();
                            }

                            dir.Refresh();
                            if (!dir.Exists)
                            {
                                return;
                            }

                            if (File.Exists(
                                $"{DirectorioDesechos}\\{DateTime.Today:dd_MM_yyyy}{log.Name}"))
                            {
                                File.Delete(
                                    $"{DirectorioDesechos}\\{DateTime.Today:dd_MM_yyyy}{log.Name}");
                            }

                            log.MoveTo(
                                $"{DirectorioDesechos}\\{DateTime.Today:dd_MM_yyyy}{log.Name}");
                            if (File.Exists(l))
                                File.Delete(l);
                            //Borrar logs de hace 2 meses
                            EliminarDisposedLogs();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMe(ex);
                }
            })
            { Priority = ThreadPriority.Lowest };
            th.Start();
        }
        private static void EliminarDisposedLogs()
        {
            DirectoryInfo dir = new DirectoryInfo($"{SQLHelper.Instance.Debugging}\\DisposedLogs");
            if (dir.Exists)
            {
                try
                {
                    dir.GetFiles().Where(o => DateTime.Now.Subtract(o.CreationTime).Days > 62).ToList().ForEach(f => f.Delete());
                }
                catch (Exception ex)
                {
                    LogMe(ex, "Eliminando logs viejos");
                }
            }
        }
        public static void LogMeSQL(string query)
        {
            try
            {
                string mensaje = string.Concat(Environment.NewLine, "--",
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "----> ", Environment.NewLine, query);
                if (SQLHelper.Instance.Debugging)
                {
                    Console.Write(mensaje);
                    return;
                }
                File.AppendAllText(Log.DBLogPath, mensaje);
            }
            catch (Exception)
            {
            }
        }
        #region Errores
        public static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                //var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // iOS: Environment.SpecialFolder.Resources
                //var errorFilePath = Path.Combine(libraryPath, Log);
                var errorMessage = $"-->[{DateTime.Now}]Error: ►►{exception.ToString()}◄◄";
                Console.WriteLine(errorMessage);
                File.WriteAllText(Log.CriticalLogPath, errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
        #endregion
        public static bool AlertOnDBConnectionError(Exception ex)
        {
            if (IsDBConnectionError(ex))
            {
                Log.OnConecctionLost?.Invoke(ex, EventArgs.Empty);
                return true;
            }
            return false;
        }
        public static bool IsDBConnectionError(Exception ex)
        {
            Exception Exbase = ex.MainExcepcion();
            Exception exception = ex;
#if NETSTANDARD
            bool desconexion = (exception is SqlException);
#else
            bool desconexion = (exception.GetType().Name == "SqlException");
#endif
            if (!desconexion)
            {
                //Asegurarse
                foreach (string identificados in new string[] { "INVALID OBJECT NAME", "FK_DESCARGAS", "INVALID COLUMN NAME", "SOCKET" })
                    if (
                        (exception?.Message?.ToUpper()?.Contains(identificados) ?? false)
                        ||
                        (ex?.Message?.ToUpper()?.Contains(identificados) ?? false)
                        ||
                        (Exbase?.Message?.ToUpper()?.Contains(identificados) ?? false)
                    )
                    {
                        //Log.LogMe($"-->[WARNING!!!] Se adapto forzadamente por:=>[☺{exception?.Message}☺,☺{ex?.Message}☺,☺{Exbase?.Message}☺]");
                        //AppData.Demonio.AdaptarLaBase();
                        //AppData.Demonio.Despierta();
                        desconexion = true;
                        break;
                    }
            }
            if (desconexion)
            {
                Log.LogMe($"-->[WARNING!!!] DESCONEXION PROVOCADA POR:=>[☺{exception?.Message}☺,☺{ex?.Message}☺,☺{Exbase?.Message}☺]");
            }
            return desconexion;


        }
    }
}
