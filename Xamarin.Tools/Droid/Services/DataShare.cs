using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Xamarin.Tools.Shared.Logging;
using Plugin.Xamarin.Tools.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Environment = Android.OS.Environment;

namespace Plugin.Xamarin.Tools.Droid.Services
{
    public class DataShare : IDataShare
    {
        Context CurrentContext => CrossCurrentActivity.Current.Activity;
        public async void ShowFile(string AttachmentName, byte[] AttachmentBytes)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            string dirPath =/* Xamarin.Forms.Forms.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).Path;*/
            Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).Path;
            var FileName = AttachmentName;
            Java.IO.File file = new Java.IO.File(dirPath, FileName);
            file.Delete();
            var filename = Path.Combine(dirPath, AttachmentName);
            File.WriteAllBytes(filename, AttachmentBytes);
            Device.BeginInvokeOnMainThread(() =>
            {
                //var oDir = Xamarin.Forms.Forms.Context.FilesDir.AbsolutePath;
                Android.Net.Uri uri = Android.Net.Uri.FromFile(file);
                Intent intent = new Intent(Intent.ActionView);
                string mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl((string)uri).ToLower());
                intent.SetDataAndType(uri, mimeType);
                intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

                try
                {
                    CurrentContext.StartActivity(intent);
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, "Al abir archivo");
                    Toast.MakeText(CurrentContext, "No cuenta con una aplicación que pueda abrir este archivo.", ToastLength.Short).Show();
                    try
                    {
                        ShareFile(file.AbsolutePath);
                    }
                    catch (Exception exx)
                    {
                        Log.LogMe(exx, "Al abir archivo desde ssegundo intento");
                        Toast.MakeText(CurrentContext, "No se pudo abrir...", ToastLength.Short).Show();
                    }

                }
            });
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
        public async void ShareFile(string title, string message, string FileName, byte[] fileData)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            string dirPath =/* Xamarin.Forms.Forms.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).Path;*/
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).Path;
            Java.IO.File file = new Java.IO.File(dirPath, FileName);
            if (file.Exists())
            {
                file.Delete();
            }
            var filename = Path.Combine(dirPath, FileName);
            File.WriteAllBytes(filename, fileData);
            ShowFile(title, message, file.AbsolutePath);
        }
        public async void ShareFiles(string title, string message, List<Tuple<string, byte[]>> Files)
        {
            if (!await CheckStoragePermission())
            {
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Debe permitir a la aplicación el acceso al almacenamiento antes de poder compartir", "Permita el acceso", "Entiendo");
                return;
            }
            string dirPath =/* Xamarin.Forms.Forms.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).Path;*/
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).Path;

            List<string> RutasArchivos = new List<string>();
            foreach (var f in Files)
            {
                Java.IO.File file = new Java.IO.File(dirPath, f.Item1);
                if (file.Exists())
                {
                    file.Delete();
                }
                var filename = Path.Combine(dirPath, f.Item1);
                File.WriteAllBytes(filename, f.Item2);
                RutasArchivos.Add(file.AbsolutePath);
            }
            ShowFiles(title, message, RutasArchivos);
            return;
        }
        public void ShowFiles(string title, string message, List<string> archivos)
        {
            string primerArchivo = archivos.First();
            var extension = primerArchivo.Substring(primerArchivo.LastIndexOf(".") + 1).ToLower();
            var contentType = string.Empty;

            // You can manually map more ContentTypes here if you want.
            switch (extension)
            {
                case "pdf":
                    contentType = "application/pdf";
                    break;
                case "png":
                    contentType = "image/png";
                    break;
                case "db":
                    contentType = "application/x-sqlite3";
                    break;
                default:
                    contentType = "application/octetstream";
                    break;
            }

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            var intent = new Intent(Intent.ActionSendMultiple);
            /////////
            intent.PutExtra(Intent.ExtraSubject, title);
            intent.SetType(contentType);
            Android.Net.Uri[] files = new Android.Net.Uri[archivos.Count];
            for (int i = 0; i < archivos.Count; i++)
            {
                string path = archivos[i];
                Java.IO.File file = new Java.IO.File(path);
                Android.Net.Uri uri = Android.Net.Uri.FromFile(file);
                files[i] = uri;
            }
            intent.PutParcelableArrayListExtra(Intent.ExtraStream, files);
            /////////
            intent.PutExtra(Intent.ExtraText, string.Empty);
            intent.PutExtra(Intent.ExtraSubject, message ?? string.Empty);

            var chooserIntent = Intent.CreateChooser(intent, title ?? string.Empty);
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(chooserIntent);
            return;
        }
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

            var extension = filePath.Substring(filePath.LastIndexOf(".") + 1).ToLower();
            var contentType = string.Empty;

            // You can manually map more ContentTypes here if you want.
            switch (extension)
            {
                case "pdf":
                    contentType = "application/pdf";
                    break;
                case "png":
                    contentType = "image/png";
                    break;
                case "db":
                    contentType = "application/x-sqlite3";
                    break;
                default:
                    contentType = "application/octetstream";
                    break;
            }

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            var intent = new Intent(Intent.ActionSend);
            intent.SetType(contentType);
            intent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse("file://" + filePath));
            intent.PutExtra(Intent.ExtraText, string.Empty);
            intent.PutExtra(Intent.ExtraSubject, message ?? string.Empty);

            var chooserIntent = Intent.CreateChooser(intent, title ?? string.Empty);
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(chooserIntent);
            return;
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
