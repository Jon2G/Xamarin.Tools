using SQLHelper;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Export;
using Stimulsoft.Report.Print;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kit.WPF.Reportes
{
    public class Reporteador
    {
        private StiReport StiReport;
        private readonly string RutaLogo;
        private readonly string RutaReportes;
        public Reporteador(string RutaLogo, string RutaReportes = "/mrt")
        {
            this.RutaLogo = RutaLogo;
            //Importante para evitar 'Error al registrar DragDrop.'
            StiOptions.Designer.PreviewReportVisible = true;
            StiOptions.Designer.AllowUseWinControl = true;
            StiOptions.Viewer.AllowUseDragDrop =
            StiOptions.Designer.AllowUseDragDrop = false;
            if (RutaReportes[0] == '/')
            {
                RutaReportes = $"{Kit.Tools.Instance.LibraryPath}{RutaReportes}";
            }
        }
        public StiReport GetStiReportObject()
        {
            return this.StiReport;
        }
        /// <summary>
        /// Carga el diccionario,compila y muestra un reporte
        /// </summary>
        /// <param name="archivo_mrt">Archivo fuente de mrt</param>
        /// <param name="diccionario">Diccionario de valores</param>
        /// <returns></returns>
        public Reporteador MostrarReporte(string archivo_mrt, params Variable[] diccionario)
        {
            FileInfo mrt = new FileInfo(archivo_mrt);
            if (!mrt.Exists)
            {
                throw new FileNotFoundException($"No se econtro el reporte en:{mrt.FullName}");
            }

            try
            {
                this.StiReport = new Stimulsoft.Report.StiReport();
                this.StiReport.Load(mrt.FullName);
                this.StiReport.Dictionary.Clear();
                DataSet data = new DataSet("TABLAS");
                foreach (Variable variable in diccionario)
                {
                    if (variable.Data.GetType() == typeof(DataTable))
                    {
                        data.Tables.Add(variable.Data as DataTable ??
                                        throw new InvalidOperationException("La tabla es nula"));
                    }
                    else
                    {
                        this.StiReport.Dictionary.Variables.Add(variable.Nombre, variable.Data);
                    }
                }

                this.StiReport.Dictionary.Variables.Add("LogoEmpresa", this.RutaLogo);
                this.StiReport.RegData(data);
                this.StiReport.Dictionary.Synchronize();
                //this.StiReport.Compile();
                //this.StiReport.Design();
                this.StiReport.Render(false);
                if (this.StiReport.IsRendered)
                {
                    this.StiReport.ShowWithRibbonGUI(true);
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex.Message);
            }
            return this;
        }
        public Reporteador MostrarReporte(Reporteador reporte)
        {

            try
            {

                if (this.StiReport.IsRendered)
                {
                    this.StiReport.ShowWithRibbonGUI(true);
                    return this;
                }
                this.StiReport.Render(false);

                if (this.StiReport.IsRendered)
                {
                    this.StiReport.ShowWithRibbonGUI(true);
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex.Message);
            }
            return this;
        }
        /// <summary>
        /// Guarda un reporte mrt en formato pdf y lo abre al finalizar
        /// </summary>
        /// <param name="carpeta">Carpeta sin slash final</param>
        /// <param name="nombre">Nombre sin extension</param>
        /// <param name="report">Reporte (es indiferente si esta compilado y renderizado o no)</param>
        private string GuardaReporte(string carpeta, string nombre, Reporteador report, bool EspacioDeTrabajo = false)
        {
            try
            {
                if (!this.StiReport.IsRendered)
                {
                    //this.StiReport.Compile();
                    this.StiReport.Render(false);
                }

                //this.StiReport.Design();
                string ruta = null;
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }
                FileInfo file = new FileInfo($"{carpeta + "\\" + nombre}.pdf");
                int i = 1;
                string alternativo = nombre;
                while (file.Exists)
                {
                    nombre = alternativo;
                    try
                    {
                        file = new FileInfo($"{carpeta + "\\" + nombre}.pdf");
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                        else
                        {
                            nombre = alternativo;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogMe(ex, $"Eliminando el reporte:{file.FullName}");
                        alternativo += i.ToString();
                        i++;
                    }
                    finally
                    {
                        try
                        {
                            Directory.GetFiles(carpeta, $"*{nombre}*.pdf",
                                SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);
                        }
                        catch (Exception ex)
                        {
                            Log.LogMe(ex, $"Eliminando reportes anteriores:{file.FullName}");
                        }
                    }
                }
                try
                {
                    StiPdfExportService service = new Stimulsoft.Report.Export.StiPdfExportService();
                    ruta = $"{carpeta + "\\" + nombre}.pdf";
                    service.ExportPdf(report.GetStiReportObject(), ruta);
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, "Al crear un reporte:" + nombre);
                }
                if (!EspacioDeTrabajo)
                {
                    try
                    {
                        Process.Start(ruta);
                        ruta = null;
                    }
                    catch (Exception ex)
                    {
                        Log.LogMe(ex);
                    }
                }

                return ruta;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al guardar el reporte:" + nombre);
                return null;
            }
        }
        /// <summary>
        /// Guarda un reporte mrt en formato excel y lo abre al finalizar
        /// </summary>
        /// <param name="carpeta">Carpeta sin slash final</param>
        /// <param name="nombre">Nombre sin extension</param>
        /// <param name="report">Reporte (es indiferente si esta compilado y renderizado o no)</param>
        private string GuardaReporteExcel(string carpeta, string nombre, Reporteador report, bool EspacioDeTrabajo = false)
        {
            try
            {
                if (!this.StiReport.IsRendered)
                {
                    //this.StiReport.Compile();
                    this.StiReport.Render(false);
                }
                nombre = nombre.Replace(".mrt", "");
                //this.StiReport.Design();
                string ruta = null;
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }
                FileInfo file = new FileInfo($"{carpeta + "\\" + nombre}.xls");
                int i = 1;
                string alternativo = nombre;
                while (file.Exists)
                {
                    nombre = alternativo;
                    try
                    {
                        file = new FileInfo($"{carpeta + "\\" + nombre}.xls");
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                        else
                        {
                            nombre = alternativo;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogMe(ex, $"Eliminando el reporte:{file.FullName}");
                        alternativo += i.ToString();
                        i++;
                    }
                    finally
                    {
                        try
                        {
                            Directory.GetFiles(carpeta, $"*{nombre}*.xls",
                                SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);
                        }
                        catch (Exception ex)
                        {
                            Log.LogMe(ex, $"Eliminando reportes anteriores:{file.FullName}");
                        }
                    }
                }
                try
                {
                    StiExcelExportService service = new Stimulsoft.Report.Export.StiExcelExportService();
                    ruta = $"{carpeta + "\\" + nombre}.xls";
                    service.ExportExcel(report.GetStiReportObject(), ruta);
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, "Al crear un reporte:" + nombre);
                }
                if (!EspacioDeTrabajo)
                {
                    try
                    {
                        Process.Start(ruta);
                        ruta = null;
                    }
                    catch (Exception ex)
                    {
                        Log.LogMe(ex);
                    }
                }

                return ruta;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al guardar el reporte:" + nombre);
                return null;
            }
        }
        /// <summary>
        /// Imprime un reporte mrt en la impresora en caso de no poder imprimirlo lo muestra en pantalla 
        /// </summary>
        /// <param name="nombre">Nombre sin extension</param>
        /// <param name="report">Reporte (es indiferente si esta compilado y renderizado o no)</param>
        public Reporteador ImprimeReporte(string nombre, string impresora, short VecesTicket = 1)
        {
            bool IniciarAlFinalizar = (impresora == "Microsoft Print to PDF");
            try
            {
                if (!this.StiReport.IsRendered)
                {
                    this.StiReport.Dictionary.Synchronize();
                    //this.StiReport.Design();
                    //this.StiReport.Reset();
                    //this.StiReport.Compile(Stimulsoft.Base.StiOutputType.WindowsApplication);
                    this.StiReport.Render(false);
                }
                else
                {

                }
                //////////////////////
                if (!IniciarAlFinalizar)
                {
                    StiPrintProvider stiPrintReport = new Stimulsoft.Report.Print.StiPrintProvider();
                    for (int i = 0; i < VecesTicket; i++)
                    {
                        this.StiReport.Print(false, 0, 1, 1, new PrinterSettings()
                        {
                            PrinterName = impresora,
                            PrintToFile = false,
                            //Copies = 1

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Al imprimir un mrt");
            }
            if (IniciarAlFinalizar)
            {
                try
                {
                    this.StiReport.ShowWithRibbonGUI(true);
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, $"Al mostrar un ticket de venta=>'{nombre}'");
                }
            }

            return this;
        }
        private string NuevoReporte(string nombre, bool Disenando = false, bool EspacioDeTrabajo = false, FormatoReporte Formato = FormatoReporte.PDF, params Variable[] variables)
        {
            this.StiReport =
                NuevoReporte(nombre, Disenando, variables)
                .GetStiReportObject();
            switch (Formato)
            {
                case FormatoReporte.EXCEL:
                    return GuardaReporteExcel($"{Kit.Tools.Instance.LibraryPath}\\HojasDeCalculo", nombre, this, EspacioDeTrabajo);
                default:
                case FormatoReporte.PDF:
                    return GuardaReporte($"{Kit.Tools.Instance.LibraryPath}\\Reportes", nombre, this, EspacioDeTrabajo);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="archivo_mrt">Nombre del archivo con extension</param>
        /// <param name="diccionario"></param>
        /// <returns></returns>
        public Reporteador NuevoReporte(string archivo_mrt, bool Disenando = false, params Variable[] diccionario)
        {
            this.StiReport = new Stimulsoft.Report.StiReport();
            if (!archivo_mrt.EndsWith(".mrt"))
            {
                if (Kit.Tools.Instance.Debugging)
                {
                    throw new ArgumentException($"Reporte sin extension");
                }
                archivo_mrt += ".mrt";
            }
            FileInfo mrt = new FileInfo($"{RutaReportes}\\{archivo_mrt}");
            if (!mrt.Exists)
            {
                throw new FileNotFoundException($"No se econtro el reporte en:{mrt.FullName}");
            }
            try
            {
                this.StiReport.Load(mrt.FullName);
                this.StiReport.Dictionary.Clear();
                DataSet data = new DataSet("TABLAS");
                foreach (Variable variable in diccionario)
                {
                    if (variable.Data?.GetType() == typeof(DataTable))
                    {
                        data.Tables.Add(variable.Data as DataTable ??
                                        throw new InvalidOperationException("La tabla es nula"));
                    }
                    else if (variable.Data is DataSet dataset)
                    {
                        this.StiReport.RegData(dataset);
                    }
                    else
                    {
                        this.StiReport.Dictionary.Variables.Add(variable.Nombre, variable.Data);
                    }
                }

                this.StiReport.Dictionary.Variables.Add("LogoEmpresa", this.RutaLogo);
                this.StiReport.RegData(data);
                this.StiReport.Dictionary.Synchronize();
                //this.StiReport.Compile();
                if (Disenando)
                {
                    Thread th = new Thread(() =>
                    {
                        this.StiReport = this.StiReport.CreateReportInNewAppDomain();
                        this.StiReport.ReportFile = mrt.FullName;
                        this.StiReport.Design(true);
                    });

                    th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                    th.Join();
                }
                this.StiReport.Render(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return this;
        }
        public static void CrearNuevoMrt(string ruta)
        {
            FileInfo file = new FileInfo(ruta);
            if (file.Extension != ".mrt")
            {
                Log.DebugMe("WARNING no se establecio la extensión al crear un nuevo reporte");
                ruta += ".mrt";
            }
            StiReport report = new StiReport();
            report.SaveDocument(ruta);
        }
    }
}

