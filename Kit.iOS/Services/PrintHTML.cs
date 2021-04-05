using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Kit.Services;
using Kit.Services.Interfaces;
using Kit.Sql;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Kit.iOS.Services.HtmlToPDF;
using Kit.Enums;
using Kit.Forms.Services.XFPDF.Helpers.Enums;
using Kit.Forms.Services.XFPDF.Model;

[assembly: Dependency(typeof(Kit.iOS.Services.PrintHTML))]
namespace Kit.iOS.Services
{
    public class PrintHTML : IPrintHTML
    {
        public bool Print(string HTML, string Printer)
        {
            try
            {
                if (Printer != "Microsoft Print to PDF")
                {
                    HTMLToPDF(HTML, Guid.NewGuid().ToString("N"));
                    return true;
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al guardar el archivo html");
                return false;
            }
            return false;
        }
        public async void HTMLToPDF(string html, string filename)
        {
            using (PDFToHtml pdf = new PDFToHtml())
            {
                pdf.HTMLString = html;
                pdf.FileName = filename;
                pdf.PageWidth = 204;
                pdf.PageHeight = 8503;
                await pdf.GeneratePDF();
                if (pdf.Status == PDFEnum.Completed)
                {
                    Print(pdf.FilePath);
                }

            }
        }
        private bool Print(string fileName)
        {
            try
            {

                UIPrintInfo printInfo = UIPrintInfo.PrintInfo;

                printInfo.OutputType = UIPrintInfoOutputType.General;
                printInfo.JobName = "TICKET";


                UIPrintInteractionController printer = UIPrintInteractionController.SharedPrintController;

                printInfo.OutputType = UIPrintInfoOutputType.General;

                printer.PrintingItem = NSUrl.FromFilename(fileName);
                printer.PrintInfo = printInfo;


                printer.ShowsPageRange = true;

                printer.Present(true, (handler, completed, err) =>
                {
                    if (!completed && err != null)
                    {
                        Log.Logger.Error($"Al imprimir ticket pdf:\n{err}");
                    }
                });

                //PrintManager printManager = (PrintManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.PrintService);
                //PrintDocumentAdapter pda = new CustomPrintDocumentAdapter(PdfFile);
                ////
                ////
                //PrintAttributes.Builder builder = new PrintAttributes.Builder();
                //var mediaSize = new PrintAttributes.MediaSize("TICKET", "TICKET", 2834, 118110);
                //builder.SetMediaSize(mediaSize);
                ////2834 (thousandths of an inch) => 72 mm
                ////118110 (thousandths of an inch) => 3000 mm
                //var job = printManager.Print("Tickets Job", pda, builder.Build());
                ////    job.Wait((long)TimeSpan.FromSeconds(30).TotalMilliseconds);
                ////

                ////

                ////Print with null PrintAttributes
                //  printManager.Print("Tickets Job", pda, null);

                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al imprimir un ticket pdf");
                return false;
            }
        }
    }
}
