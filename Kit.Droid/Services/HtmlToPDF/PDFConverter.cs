using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Android.Webkit;
using Kit.Enums;
using Kit.Services;
using Kit.Services.Interfaces;
using static Kit.Services.PDFToHtml;
using Kit.Sql;

namespace Kit.Droid.Services.HtmlToPDF
{
    public class PDFConverter : IPDFConverter
    {
        public void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml)
        {
            try
            {
                WebView webpage = new Android.Webkit.WebView(Android.App.Application.Context);
                webpage.Settings.JavaScriptEnabled = true;

#pragma warning disable CS0618 // Type or member is obsolete
                webpage.DrawingCacheEnabled = true;
#pragma warning restore CS0618 // Type or member is obsolete

                webpage.SetLayerType(LayerType.Software, null);
                webpage.Layout(0, 0, (int)_PDFToHtml.PageWidth, (int)_PDFToHtml.PageHeight);
                webpage.LoadData(_PDFToHtml.HTMLString, "text/html; charset=utf-8", "UTF-8");
                webpage.SetWebViewClient(new WebViewCallBack(_PDFToHtml));
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "ConvertHTMLtoPDF");
                _PDFToHtml.Status = PDFEnum.Failed;
            }
        }
    }
}
