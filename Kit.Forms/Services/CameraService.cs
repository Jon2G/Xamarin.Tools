using System;
using System.IO;
using System.Threading.Tasks;
using Kit.Forms.Services.Interfaces;
using Xamarin.Essentials;

namespace Kit.Forms.Services
{
    public static class CameraService
    {
        public static async Task<FileInfo> TakePhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                var file = await LoadPhotoAsync(photo);
                Console.WriteLine($"CapturePhotoAsync COMPLETED: {file}");
                return file;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature is now supported on the device
            }
            catch (PermissionException pEx)
            {
                // Permissions not granted
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }

            return null;
        }

        public static async Task<FileInfo> LoadPhotoAsync(this FileResult photo)
        {
            // canceled
            if (photo == null)
            {
                await Task.Yield();
                return null;
            }
            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
                await stream.CopyToAsync(newStream);

            return new FileInfo(newFile);
        }
    }
}
