using System;
using System.Threading.Tasks;
using System.IO;
using Xamarin.Forms;
using UIKit;
using QuickLook;
using Tools.Services.Interfaces;
using Tools.iOS.Classes;

namespace Tools.iOS.Services
{
    public class PDFSaveAndOpen: IPDFSaveAndOpen
    {
        public async Task SaveAndView(string fileName, MemoryStream stream, PDFOpenContext context = PDFOpenContext.InApp, string contentType = "application / pdf")
        {
            await Task.Yield();
            //Get the root path in iOS device.
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, fileName);

            //Create a file and write the stream into it.
            FileStream fileStream = File.Open(filePath, FileMode.Create);
            stream.Position = 0;
            stream.CopyTo(fileStream);
            fileStream.Flush();
            fileStream.Close();

            //Invoke the saved document for viewing
            UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (currentController.PresentedViewController != null)
                currentController = currentController.PresentedViewController;
            UIView currentView = currentController.View;

            QLPreviewController qlPreview = new QLPreviewController();
            QLPreviewItem item = new QLPreviewItemBundle(fileName, filePath);
            qlPreview.DataSource = new PreviewControllerDS(item);

            currentController.PresentViewController(qlPreview, true, null);
        }
    }
}
