using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Kit.Enums;
using Kit.Services;
using Xamarin.Forms;
using static Kit.Services.PDFToHtml;

namespace Kit.Droid.Services.HtmlToPDF
{
    public class WebViewCallBack : WebViewClient
    {
        protected bool _complete;
        protected readonly PDFToHtml pDFToHtml;

        public WebViewCallBack(PDFToHtml _PDFToHtml)
        {
            pDFToHtml = _PDFToHtml;
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            if (!_complete)
            {
                _complete = true;

                Device.BeginInvokeOnMainThread(() =>
                {
                    OnPageLoaded(view);
                });
            }
        }
        public override void OnLoadResource(Android.Webkit.WebView view, string url)
        {
            base.OnLoadResource(view, url);
            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                if (!_complete)
                    OnPageFinished(view, url);
                return false;
            });
        }

        internal void OnPageLoaded(Android.Webkit.WebView webView)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    PrintAttributes.Builder builder = new PrintAttributes.Builder();
                    builder.SetMediaSize(PrintAttributes.MediaSize.IsoA4);
                    builder.SetResolution(new PrintAttributes.Resolution("pdf", "pdf", (int)pDFToHtml.PageDPI, (int)pDFToHtml.PageDPI));
                    builder.SetMinMargins(PrintAttributes.Margins.NoMargins);
                    PrintAttributes attributes = builder.Build();
                    PrintDocumentAdapter? adapter = webView.CreatePrintDocumentAdapter(pDFToHtml.FileName);
                    PdfLayoutResultCallback layoutResultCallback = new PdfLayoutResultCallback();
                    layoutResultCallback.Adapter = adapter;
                    layoutResultCallback.PDFToHtml = pDFToHtml;
                    adapter.OnLayout(null, attributes, null, layoutResultCallback, null);
                }
            }
            catch
            {
                pDFToHtml.Status = PDFEnum.Failed;
            }
        }
        public override void OnReceivedError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceError error)
        {
            base.OnReceivedError(view, request, error);
            pDFToHtml.Status = PDFEnum.Failed;
        }

        public override void OnReceivedHttpError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            base.OnReceivedHttpError(view, request, errorResponse);
            pDFToHtml.Status = PDFEnum.Failed;
        }
    }
}
