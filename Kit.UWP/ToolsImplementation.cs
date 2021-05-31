using System;
using System.Diagnostics;
using Kit.Enums;
using Kit.Forms.Controls.CrossImage;
using Kit.Forms.Services;
using Kit.UWP.Services;
using Windows.Storage;
using CustomMessageBoxService = Kit.UWP.Services.CustomMessageBoxService;

namespace Kit.UWP
{
    public class ToolsImplementation : AbstractTools
    {
        private string _LibraryPath;
        public override void Init()
        {
            base.Init(new CustomMessageBoxService(), new SynchronizeInvoke(),
                new ScreenManagerService(), new ImageExtensions(), null);
            //StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFolder file = storageFolder.CreateFolderAsync("KitData",
                 Windows.Storage.CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
            _LibraryPath = file.Path;
        }
 
        public override string LibraryPath => _LibraryPath;
        
        public override RuntimePlatform RuntimePlatform { get=>RuntimePlatform.UWP; }

        #region UWP Especific
        public static ToolsImplementation UWPInstance
        {
            get => Kit.Tools.Instance as ToolsImplementation;
        }
        private bool? _IsInDesingMode;
        public new bool IsInDesingMode
        {
            get
            {
                if (_IsInDesingMode is null)
                {
                    _IsInDesingMode = Designing();
                }
                return (bool)_IsInDesingMode;
            }
        }

     

        private bool Designing()
        {
            string name = Process.GetCurrentProcess().ProcessName;
            name = name?.Trim()?.ToUpper();
            if (name == "XDESPROC" || name == "DEVENV")
            {
                return true;
            }
            // MessageBox.Show(name);
            return false;
        }
        #endregion
    }
}
