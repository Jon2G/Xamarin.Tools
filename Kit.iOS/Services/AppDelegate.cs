using Acr.UserDialogs;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Transformations;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Kit.iOS.Services
{
    public class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public bool FinishedLaunching<T>(UIApplication app, NSDictionary options) where T : Xamarin.Forms.Application
        {
            global::Xamarin.Forms.Forms.SetFlags("Expander_Experimental", "Brush_Experimental", "Shapes_Experimental", "DragAndDrop_Experimental");
            //global::Xamarin.Forms.Forms.Init();
            //global::Xamarin.Forms.FormsMaterial.Init();

            new TintTransformation();
            Kit.iOS.Tools.Init().Init(Kit.Tools.Instance.LibraryPath, true);
            LoadApplication((Xamarin.Forms.Application)Activator.CreateInstance(typeof(T)));
            return base.FinishedLaunching(app, options);
        }
    }
}