using FFImageLoading.Transformations;
using Foundation;
using Kit.Forms.Services;
using Kit.Forms.Services.Interfaces;
using System;
using UIKit;
using Xamarin.Forms;

namespace Kit.iOS.Services
{
    public abstract class AppDelegate<T> : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUpdateWidget
        where T : Application
    {
        public AppDelegate()
        {

        }

        protected virtual void Initialize() { FontCache.DeleteFontCacheIfFontChanged(typeof(T)); }
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public virtual void BeforeLoadApplication(UIApplication app, NSDictionary options) { }
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.SetFlags("Shapes_Experimental", "DragAndDrop_Experimental");
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.FormsMaterial.Init();
            Rg.Plugins.Popup.Popup.Init();
            Forms9Patch.iOS.Settings.Initialize(this);
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            Initialize();
            new TintTransformation();
            Kit.iOS.Tools.Init();
            TinyIoC.TinyIoCContainer.Current.Register<IImageCompressService>(new ImageCompressService());
            TinyIoC.TinyIoCContainer.Current.Register<IUpdateWidget>(this);
            BeforeLoadApplication(app, options);
            LoadApplication(Activator.CreateInstance<T>());
            return base.FinishedLaunching(app, options);
        }

        public virtual void UpdateWidget(string AppWidgetProviderClassName)
        {
            throw new System.NotImplementedException();
        }
    }
}