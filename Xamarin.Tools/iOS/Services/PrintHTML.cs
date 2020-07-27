﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Plugin.Xamarin.Tools.Shared.Services;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using SQLHelper;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.Xamarin.Tools.iOS.Services
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
                SQLHelper.Log.LogMe(ex, "Al guardar el archivo html");
                return false;
            }
            return false;
        }
        public void HTMLToPDF(string html, string filename)
        {
            using (PDFToHtml pdf = new PDFToHtml(html, filename))
            {
                pdf.PageWidth = 204;
                pdf.PageHeight = 8503;
                pdf.OnCompleted += PdfCompleted;
                pdf.GeneratePDF();

            }
        }
        private void PdfCompleted(object sender, EventArgs e)
        {
            if (sender is PDFToHtml pdf)
            {
                if (pdf.Status == PDFToHtml.PDFEnum.Completed)
                {
                    Print(pdf.FilePath);
                }
            }
        }
        private bool Print(string fileName)
        {
            try
            {

                var printInfo = UIPrintInfo.PrintInfo;

                printInfo.OutputType = UIPrintInfoOutputType.General;
                printInfo.JobName = "TICKET";


                var printer = UIPrintInteractionController.SharedPrintController;

                printInfo.OutputType = UIPrintInfoOutputType.General;

                printer.PrintingItem = NSUrl.FromFilename(fileName);
                printer.PrintInfo = printInfo;


                printer.ShowsPageRange = true;

                printer.Present(true, (handler, completed, err) =>
                {
                    if (!completed && err != null)
                    {
                        SQLHelper.Log.LogMe($"Al imprimir ticket pdf:\n{err}");
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
                Log.LogMe(ex, "Al imprimir un ticket pdf");
                return false;
            }
        }
    }
}
