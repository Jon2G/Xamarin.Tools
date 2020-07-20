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

namespace Plugin.Xamarin.Tools.WPF.Services
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
                PdfGenerateConfig config = new PdfGenerateConfig();
                config.ManualPageSize = new PdfSharp.Drawing.XSize(204, 283);
                //config.ManualPageSize = new PdfSharp.Drawing.XSize(226, 140);
                config.SetMargins(5);


                using (PdfDocument pdf = PdfGenerator.GeneratePdf(HTML, config))
                {
                    pdf.Save(pdfile.FullName);
                }
                if (pdfile.Exists)
                {
                    return PrintPDF(Printer, "A4", pdfile.FullName, 1);
                }

            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al convertir un ticket html a pdf");
            }
            return false;
        }
        private bool PrintPDF(string printer, string paperName, string filename, int copies)
        {
            try
            {
                // Now print the PDF document
                using (PdfiumViewer.PdfDocument document = PdfiumViewer.PdfDocument.Load(filename))
                {
                    using (PrintDocument printDocument = document.CreatePrintDocument(PdfiumViewer.PdfPrintMode.ShrinkToMargin))
                    {
                        ////////////////
                        PaperSize ps = new PaperSize("80", 3, 3);
                        printDocument.DefaultPageSettings.Margins.Left = 0;
                        printDocument.DefaultPageSettings.Margins.Right = 0;
                        printDocument.DefaultPageSettings.Margins.Top = 0;
                        printDocument.DefaultPageSettings.Margins.Bottom = 0;
                        printDocument.DefaultPageSettings.PaperSize = ps;
                        printDocument.PrintController = new StandardPrintController();
                        printDocument.PrinterSettings = new PrinterSettings() { PrinterName = printer, Copies = (short)copies };

                        //printDocument.PrinterSettings.PrintToFile = true;
                        //printDocument.PrinterSettings.PrintFileName = filename.Replace(".pdf", "_PRINT.pdf");
                        ////////////////
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
