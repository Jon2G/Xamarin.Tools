using Acr.UserDialogs;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Transformations;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;

namespace Kit.iOS.Services
{
    public abstract class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        protected abstract Application GetApp { get; }
        protected abstract void Initialize();
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.SetFlags("Shapes_Experimental", "DragAndDrop_Experimental");
            global::Xamarin.Forms.Forms.Init();
            Rg.Plugins.Popup.Popup.Init();
            Forms9Patch.iOS.Settings.Initialize(this);
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            Initialize();
            new TintTransformation();
            Kit.iOS.Tools.Init();
            LoadApplication(GetApp);
            return base.FinishedLaunching(app, options);
        }
    }
}