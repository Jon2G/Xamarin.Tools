using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Xamarin.Tools.Shared.Logging
{
    public static class Log
    {
        public static string LogDirectory { get; private set; }
        public static string LogPath { get; private set; }
        public static string BackgroundLogPath { get; private set; }
        public static string DBLogPath { get; private set; }
        public static string CriticalLogPath { get; private set; }

        public static void Init(string LogDirectory, bool AlertAfterCritical = false)
        {
            Log.LogDirectory = LogDirectory;
            Log.CriticalLogPath = $"{Log.LogDirectory}\\Critical.Log";
            Log.LogPath = $"{Log.LogDirectory}\\AppData.Log";
            Log.BackgroundLogPath = $"{Log.LogDirectory}\\AppDataDemonio.Log";
            Log.DBLogPath = $"{Log.LogDirectory}\\SQL_T.sql";
            if (!Directory.Exists(Log.LogDirectory))
            {
                Directory.CreateDirectory(Log.LogDirectory);
            }
            if (AlertAfterCritical)
            {

            }
        }

        public static void ChangeLogPath(string Path)
        {
            Log.BackgroundLogPath = $"{Path}\\AppDataDemonio.Log";
            Log.LogPath = $"{Path}\\AppData.Log";
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

        public static void LogMe(string error)
        {
            try
            {
                string mensaje = string.Concat(Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "---->", error);
                if (Tools.Debugging)
                {
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
            if (error.GetType().Name == "SDKException")
            {
                //ignorar errores del lector n.n
                return;
            }
            try
            {
                string mensaje = string.Concat(Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "----> ", error.Message);
                if (Tools.Debugging)
                {
                    Console.Write(mensaje);
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
                if (Tools.Debugging)
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
                if (Tools.Debugging)
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
        public static void LogMeDemonio(string error)
        {
            try
            {
                string mensaje = string.Concat(Environment.NewLine,
                    DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "----> ", error);
                if (Tools.Debugging)
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
            DirectoryInfo dir = new DirectoryInfo($"{Tools.Debugging}\\DisposedLogs");
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
                if (Tools.Debugging)
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
    }
}
