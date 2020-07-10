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
using Android.Text;
using System.IO;

namespace Plugin.Xamarin.Tools.Droid.Services
{
    public class PrintHTML : IPrintHTML
    {
        public bool Print(string HTML, string Printer)
        {
            try
            {
                if (Printer != "Microsoft Print to PDF")
                {
                    if (Forms9Patch.ToPdfService.IsAvailable)
                    {
                        Forms9Patch.PrintService.PrintAsync(HTML, "XCOMANDERA");
                    }
                    else
                        Acr.UserDialogs.UserDialogs.Instance.Toast("PDF Export is not available on this device", TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception ex)
            {
                SQLHelper.Log.LogMe(ex, "Al guardar el archivo html");
                return false;
            }
            return false;
        }

    }
}
