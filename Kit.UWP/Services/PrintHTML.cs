

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;
using Windows.UI.Xaml.Printing;

namespace Kit.UWP.Services
{
    public class PrintHTML : IPrintHTML
    {

        public bool Print(string HTML, string Printer)
        {
            try
            {
                DirectoryInfo TicketsPath = new DirectoryInfo(Kit.Tools.Instance.LibraryPath + "\\TICKETS");
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
        private bool PrintToPDF(string HTML, string TicketPath, string Printer)
        {
            try
            {
                FileInfo pdfile = new FileInfo($"{TicketPath}.pdf");


                //HtmlToPdf Renderer = new HtmlToPdf();
                //Renderer.PrintOptions.SetCustomPaperSizeinMilimeters(72, 3000);
                //Renderer.RenderHtmlAsPdf(HTML)
                //    .SaveAs(pdfile.FullName);


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
                //PdfDocument Pdf = PdfDocument.FromFile(filename);
                //Pdf.Print(printer);

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
