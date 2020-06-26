using Plugin.Xamarin.Tools.Shared.Services;
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
namespace Plugin.Xamarin.Tools.Droid.Services
{
   public class PrintHTML: IPrintHTML
    {
        public void Print(WebView viewToPrint)
        {
            Activity activity = null;//(FormatoTexto.Platforms.Shared.FormatoTexto.Activity as Activity);

            IVisualElementRenderer renderer = Platform.CreateRendererWithContext(viewToPrint, activity);
            var webView = renderer.ViewGroup.GetChildAt(0) as DroidWebView;


            if (webView != null)
            {
                var version = Build.VERSION.SdkInt;

                if (version >= BuildVersionCodes.Kitkat)
                {
                    string name = Guid.NewGuid().ToString("N");
                    PrintDocumentAdapter documentAdapter = webView.CreatePrintDocumentAdapter(name);
                    var printMgr = (PrintManager)activity.GetSystemService(Context.PrintService);
                    printMgr.Print("FormatoTexto-Print", documentAdapter, null);
                }
            }
        }
    }
}
