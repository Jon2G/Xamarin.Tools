using CoreGraphics;
using Plugin.Xamarin.Tools.Shared.Services;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WebKit;
using static Plugin.Xamarin.Tools.Shared.Services.PDFToHtml;

namespace Plugin.Xamarin.Tools.iOS.Services.HtmlToPDF
{
    internal class PDFConverter : IPDFConverter
    {
        public void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml)
        {
            try
            {
                WKWebView webView = new WKWebView(new CGRect(0, 0, (int)_PDFToHtml.PageWidth, (int)_PDFToHtml.PageHeight), new WKWebViewConfiguration());
                webView.UserInteractionEnabled = false;
                webView.BackgroundColor = UIColor.White;
                webView.NavigationDelegate = new WebViewCallBack(_PDFToHtml);
                webView.LoadHtmlString(_PDFToHtml.HTMLString, null);
            }
            catch
            {
                _PDFToHtml.Status = PDFEnum.Failed;
            }
        }
    }
}
