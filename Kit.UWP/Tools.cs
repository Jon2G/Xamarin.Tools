using System;
using System.Threading.Tasks;
using Windows.Storage;
using Kit;

namespace Tools.UWP
{
    /// <summary>
    /// Interface for Xamarin.Tools
    /// </summary>
    public partial class Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init()
        {
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Kit.Tools.Set(new ToolsImplementation());
            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFolder file = storageFolder.CreateFolderAsync("KitData",
                 Windows.Storage.CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
            Kit.Tools.Instance.SetLibraryPath(file.Path);


            //     ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();


            #region DependencyServices
            //DependencyService.Register<DataShare>();
            //DependencyService.Register<PDFSaveAndOpen>();
            //DependencyService.Register<PhotoPickerService>();
            //DependencyService.Register<PrintHTML>();
            //DependencyService.Register<Services.DeviceInfo>();
            // DependencyService.Register<Screenshot>();
            //DependencyService.Register<CustomMessageBoxService>();
            #endregion
            return Kit.Tools.Instance;
        }

        //static AbstractTools currentInstance;
        //public static AbstractTools Instance
        //{
        //    get
        //    {
        //        if (currentInstance == null)
        //            throw new ArgumentException("[Shared.Tools] In android, you must call UserDialogs.Init(Activity) from your first activity OR UserDialogs.Init(App) from your custom application OR provide a factory function to get the current top activity via UserDialogs.Init(() => supply top activity)");

        //        return currentInstance;
        //    }
        //    set => currentInstance = value;
        //}
    }
}
