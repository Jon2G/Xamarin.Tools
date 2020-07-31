
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Printing;

namespace Plugin.Xamarin.Tools.UWP.Services
{
    internal class PrintHTML : IPrintHTML
    {

        public bool Print(string HTML, string Printer)
        {
            try
            {
                DirectoryInfo TicketsPath = new DirectoryInfo(Shared.Tools.Instance.LibraryPath + "\\TICKETS");
                if (!TicketsPath.Exists)
                {
                    TicketsPath.Create();
                }
                TicketsPath.Refresh();
                if (!TicketsPath.Exists)
                {
                    return false;
                }
                string TicketPath = $"{TicketsPath.FullName}\\{Guid.NewGuid():N}";
                if (Printer == "Microsoft Print to PDF")
                {
                    FileInfo file = new FileInfo($"{TicketPath}.html");
                    File.WriteAllText(file.FullName, HTML);
                    file.Refresh();
                    if (file.Exists)
                    {
                        Process.Start(file.FullName);
                        return true;
                    }
                }
                else
                {
                    return PrintToPDF(HTML, TicketPath, Printer);
                }
            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al guardar el archivo html");
                return false;
            }
            return false;
        }
        private  bool PrintToPDF(string HTML, string TicketPath, string Printer)
        {
            try
            {
                FileInfo pdfile = new FileInfo($"{TicketPath}.pdf");
                //MemoryStream ms = new MemoryStream();
                //ConverterProperties converterProperties = new ConverterProperties();
                //HtmlConverter.ConvertToPdf(HTML, ms, converterProperties);

                //var writer = new iText.Kernel.Pdf.PdfWriter(pdfile.FullName, new WriterProperties());
                //iText.Kernel.Pdf.PdfDocument vDoc = new iText.Kernel.Pdf.PdfDocument(writer);
                //vDoc.SetDefaultPageSize(new iText.Kernel.Geom.PageSize(204, 8503));
                //vDoc.Close();

                if (pdfile.Exists)
                {
                    return PrintPDF(Printer, pdfile.FullName, 1);

                }

            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al convertir un ticket html a pdf");
            }
            return false;
        }
        private bool PrintPDF(string printer, string filename, int copies)
        {
            try
            {
                // Now print the PDF document
                ProcessStartInfo info = new ProcessStartInfo();
                info.Verb = $"print /d:{printer}";
                info.FileName = filename;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                Process p = new Process();
                p.StartInfo = info;
                p.Start();

                p.WaitForInputIdle();
                System.Threading.Thread.Sleep(3000);
                if (false == p.CloseMainWindow())
                    p.Kill();

                return true;
            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al imprimir un pdf");
            }
            return false;
        }
    }
}
