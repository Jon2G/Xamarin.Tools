﻿using Kit.iOS.Classes;
using Kit.iOS.Services;
using QuickLook;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PDFSaveAndOpen))]
namespace Kit.iOS.Services
{
    public class PDFSaveAndOpen
    {
        public async Task SaveAndView(string fileName, MemoryStream stream, string contentType = "application / pdf")
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
