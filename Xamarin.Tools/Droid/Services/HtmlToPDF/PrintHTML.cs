using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using DroidWebView = Android.Webkit.WebView;
using Android.Text;
using System.IO;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using SQLHelper;
using System.Threading.Tasks;
using Plugin.CurrentActivity;
using Plugin.Xamarin.Tools.Shared.Services;
using Android.Webkit;
using Android.Graphics.Pdf;
using Java.IO;

namespace Plugin.Xamarin.Tools.Droid.Services.HtmlToPDF
{
    public class PrintHTML : IPrintHTML
    {
        public async Task<bool> Print(string HTML, string Printer)
        {
            try
            {
                await Task.Yield();
                if (Printer != "Microsoft Print to PDF")
                {
                    HTMLToPDF(HTML, Guid.NewGuid().ToString("N"));
                    //if (!string.IsNullOrEmpty(pdf))
                    //{
                    //    return Print(pdf);
                    //}
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

        private bool Print(string PdfFile)
        {
            try
            {
                PrintManager printManager = (PrintManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.PrintService);
                PrintDocumentAdapter pda = new CustomPrintDocumentAdapter(PdfFile);
                //
                //
                PrintAttributes.Builder builder = new PrintAttributes.Builder();
                var mediaSize= new PrintAttributes.MediaSize("TICKET", "TICKET", 2834, 118110);
                builder.SetMediaSize(mediaSize);
                //2834 (thousandths of an inch) => 72 mm
                //118110 (thousandths of an inch) => 3000 mm
                var job = printManager.Print("Tickets Job", pda, builder.Build());
                //    job.Wait((long)TimeSpan.FromSeconds(30).TotalMilliseconds);
                //

                //

                //Print with null PrintAttributes
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
