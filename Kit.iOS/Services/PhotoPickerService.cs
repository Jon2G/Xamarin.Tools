using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Kit.Services.Interfaces;
using SQLHelper;
using UIKit;
using Xamarin.Forms;

namespace Kit.iOS.Services
{
    public class PhotoPickerService : IPhotoPickerService
    {
        TaskCompletionSource<Tuple<byte[], ImageSource>> taskCompletionSource;
        UIImagePickerController imagePicker;
        void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;

            if (image != null)
            {
                // Convert UIImage to .NET Stream object
                NSData data;
                if (args.ReferenceUrl.PathExtension.Equals("PNG") || args.ReferenceUrl.PathExtension.Equals("png"))
                {
                    data = image.AsPNG();
                }
                else
                {
                    data = image.AsJPEG(1);
                }
                Stream stream = data.AsStream();

                UnregisterEventHandlers();


                byte[] bits = null;
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        bits = memoryStream.ToArray();
                    }
                    Tuple<byte[], ImageSource> tuple = new Tuple<byte[], ImageSource>(bits, ImageSource.FromStream(() => stream));
                    // Set the Stream as the completion of the Task
                    taskCompletionSource.SetResult(tuple);
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, "Al obtener la imagen despues de ser abierta");
                }
            }
            else
            {
                UnregisterEventHandlers();
                taskCompletionSource.SetResult(null);
            }
            imagePicker.DismissModalViewController(true);
        }
        void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            taskCompletionSource.SetResult(null);
            imagePicker.DismissModalViewController(true);
        }
        void UnregisterEventHandlers()
        {
            imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled -= OnImagePickerCancelled;
        }
        public Task<Tuple<byte[], ImageSource>> GetImageAsync()
        {
            // Create and define UIImagePickerController
            imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            // Set event handlers
            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCancelled;

            // Present UIImagePickerController;
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentModalViewController(imagePicker, true);

            // Return Task object
            taskCompletionSource = new TaskCompletionSource<Tuple<byte[], ImageSource>>();
            return taskCompletionSource.Task;
        }
    }
}
