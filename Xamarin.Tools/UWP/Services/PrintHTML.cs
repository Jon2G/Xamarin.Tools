using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.UWP.Services
{
    public class PrintHTML : Shared.Services.IPrintHTML
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
            catch(Exception ex)
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
                PdfGenerateConfig config = new PdfGenerateConfig();
                config.ManualPageSize = new PdfSharp.Drawing.XSize(226,int.MaxValue);
                config.MarginBottom =
                    config.MarginTop =
                    config.MarginLeft =
                    config.MarginRight = 0;

                using (PdfDocument pdf = PdfGenerator.GeneratePdf(HTML, config))
                {
                    pdf.Save(pdfile.FullName);
                }
                if (pdfile.Exists)
                {
                    return PrintPDF(Printer, "80mm", pdfile.FullName, 1);
                }

            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al convertir un ticket html a pdf");
            }
            return false;
        }
        private bool PrintPDF(string printer,string paperName,string filename,int copies)
        {
            try
            {
                // Create the printer settings for our printer
                var printerSettings = new PrinterSettings
                {
                    PrinterName = printer,
                    Copies = (short)copies,
                };

                // Create our page settings for the paper size selected
                var pageSettings = new PageSettings(printerSettings)
                {
                    Margins = new Margins(0, 0, 0, 0),
                };
                if (paperName == "58mm")
                {
                    pageSettings.PaperSize = new PaperSize("TK", 2, int.MaxValue);
                }
                else if (paperName == "80mm")
                {
                    pageSettings.PaperSize = new PaperSize("TK", 3, int.MaxValue);
                }
                else
                {
                    foreach (PaperSize paperSize in printerSettings.PaperSizes)
                    {
                        if (paperSize.PaperName == paperName)
                        {
                            pageSettings.PaperSize = paperSize;
                            break;
                        }
                    }
                }
                // Now print the PDF document
                using (var document = PdfiumViewer.PdfDocument.Load(filename))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.DefaultPageSettings = pageSettings;
                        printDocument.PrintController = new StandardPrintController();
                        printDocument.Print();
                    }
                }
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
