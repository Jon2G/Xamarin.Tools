using Foundation;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Xamarin.Tools.Shared.Logging;
using Plugin.Xamarin.Tools.Shared.Services;
using QuickLook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.iOS.Services
{
    public class DataShare : IDataShare
    {
        public void ShowFile(string AttachmentName, byte[] AttachmentBytes)
        {
            var FileName = AttachmentName;
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var filename = Path.Combine(dirPath, FileName);
            FileInfo fi = new FileInfo(filename);

            if (!NSFileManager.DefaultManager.FileExists(filename))
            {
                Stream stream = new MemoryStream(AttachmentBytes);
                NSData imgData = NSData.FromStream(stream);
                NSError err;
                imgData.Save(filename, false, out err);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                QLPreviewController previewController = new QLPreviewController();
                previewController.DataSource = new PDFPreviewControllerDataSource(fi.FullName, fi.Name);
                UINavigationController controller = FindNavigationController();
                if (controller != null)
                    controller.PresentViewController(previewController, true, null);
            });

        }
        private UINavigationController FindNavigationController()
        {
            foreach (var window in UIApplication.SharedApplication.Windows)
            {
                if (window.RootViewController.NavigationController != null)
                    return window.RootViewController.NavigationController;
                else
                {
                    UINavigationController val = CheckSubs(window.RootViewController.ChildViewControllers);
                    if (val != null)
                        return val;
                }
            }

            return null;
        }
        private UINavigationController CheckSubs(UIViewController[] controllers)
        {
            foreach (var controller in controllers)
            {
                if (controller.NavigationController != null)
                    return controller.NavigationController;
                else
                {
                    UINavigationController val = CheckSubs(controller.ChildViewControllers);
                    if (val != null)
                        return val;
                }
            }
            return null;
        }
        public void ShareFile(string absolutePath)
        {
            try
            {
               ShowFile("Abrir con:", "Seleccione", absolutePath);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, $"Al compartir el archivo:'{absolutePath}'");
            }
        }

        public class PDFItem : QLPreviewItem
        {
            string title;
            string uri;

            public PDFItem(string title, string uri)
            {
                this.title = title;
                this.uri = uri;
            }

            public override string ItemTitle
            {
                get { return title; }
            }

            public override NSUrl ItemUrl
            {
                get { return NSUrl.FromFilename(uri); }
            }
        }
        public class PDFPreviewControllerDataSource : QLPreviewControllerDataSource
        {
            string url = "";
            string filename = "";

            public PDFPreviewControllerDataSource(string url, string filename)
            {
                this.url = url;
                this.filename = filename;
            }

            public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
            {
                return (IQLPreviewItem)new PDFItem(filename, url);
            }

            public override nint PreviewItemCount(QLPreviewController controller)
            {
                return 1;
            }
        }

        public async void ShareFile(string title, string message, string FileName, byte[] fileData)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var filename = Path.Combine(dirPath, FileName);
            FileInfo fi = new FileInfo(filename);
            if (!NSFileManager.DefaultManager.FileExists(filename))
            {
                Stream stream = new MemoryStream(fileData);
                NSData imgData = NSData.FromStream(stream);
                NSError err;
                imgData.Save(filename, false, out err);
            }
            var items = new NSObject[] { NSObject.FromObject(title), NSUrl.FromFilename(fi.FullName) };
            var activityController = new UIActivityViewController(items, null);
            var vc = GetVisibleViewController();
            NSString[] excludedActivityTypes = null;

            if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
                activityController.ExcludedActivityTypes = excludedActivityTypes;
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (activityController.PopoverPresentationController != null)
                {
                    activityController.PopoverPresentationController.SourceView = vc.View;
                }
            }
            Device.BeginInvokeOnMainThread(async () =>
            {
                await vc.PresentViewControllerAsync(activityController, true);
            });
            ShowFile(title, message, FileName);
            return;
        }

        // MUST BE CALLED FROM THE UI THREAD
        public async void ShowFile(string title, string message, string filePath)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("No se encontro el archivo", "Mensaje informativo");
                return;
            }

            var items = new NSObject[] { NSObject.FromObject(title), NSUrl.FromFilename(filePath) };
            var activityController = new UIActivityViewController(items, null);
            var vc = GetVisibleViewController();

            NSString[] excludedActivityTypes = null;

            if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
                activityController.ExcludedActivityTypes = excludedActivityTypes;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (activityController.PopoverPresentationController != null)
                {
                    activityController.PopoverPresentationController.SourceView = vc.View;
                }
            }
            await vc.PresentViewControllerAsync(activityController, true);
        }

        public async void ShareFiles(string title, string message, List<Tuple<string, byte[]>> Files)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            foreach (Tuple<string, byte[]> archivo in Files)
            {
                string filename = Path.Combine(dirPath, archivo.Item1);
                FileInfo fi = new FileInfo(filename);
                if (!NSFileManager.DefaultManager.FileExists(filename))
                {
                    Stream stream = new MemoryStream(archivo.Item2);
                    NSData imgData = NSData.FromStream(stream);
                    NSError err;
                    imgData.Save(filename, false, out err);
                }
            }
            NSObject[] items = new NSObject[Files.Count * 2];
            for (int i = 0; i < Files.Count; i += 2)
            {
                Tuple<string, byte[]> archivo = Files[i];
                items[i] = NSObject.FromObject(title);
                items[i + 1] = NSUrl.FromFilename(archivo.Item1);
            }

            UIActivityViewController activityController = new UIActivityViewController(items, null);
            UIViewController vc = GetVisibleViewController();
            NSString[] excludedActivityTypes = null;

            if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
                activityController.ExcludedActivityTypes = excludedActivityTypes;
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (activityController.PopoverPresentationController != null)
                {
                    activityController.PopoverPresentationController.SourceView = vc.View;
                }
            }
            Device.BeginInvokeOnMainThread(async () =>
            {
                await vc.PresentViewControllerAsync(activityController, true);
            });
            ShowFiles(title, message, Files.Select(x => x.Item1).ToList());
            return;
        }
        public async void ShowFiles(string title, string message, List<string> archivos)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            NSObject[] items = new NSObject[archivos.Count * 2];
            for (int i = 0; i < archivos.Count; i += 2)
            {
                items[i] = NSObject.FromObject(title);
                items[i + 1] = NSUrl.FromFilename(archivos[i]);
            }

            var activityController = new UIActivityViewController(items, null);
            var vc = GetVisibleViewController();

            NSString[] excludedActivityTypes = null;

            if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
                activityController.ExcludedActivityTypes = excludedActivityTypes;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (activityController.PopoverPresentationController != null)
                {
                    activityController.PopoverPresentationController.SourceView = vc.View;
                }
            }
            await vc.PresentViewControllerAsync(activityController, true);
        }

        UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).TopViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }

        public async Task<bool> CheckStoragePermission()
        {
            PermissionStatus status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Permita el acceso al almacentamiento", "Almacenamiento", "OK");
                }
                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
            }
            return status == PermissionStatus.Granted;
        }
    }
}
