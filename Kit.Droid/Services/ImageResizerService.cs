using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kit.Droid.Services;
using Kit.Forms.Services.Interfaces;
using Xamarin.Forms;
using Android.Graphics;
using FFImageLoading;

[assembly: Dependency(typeof(ImageResizerService))]
namespace Kit.Droid.Services
{
    public class ImageResizerService : IImageResizer
	{
        public static FileStream ResizeImageAndroid(Stream imageData, float width, float height)
        {
            // Load the bitmap
            byte[] bytes = imageData.ToByteArray();
            Bitmap originalImage = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);
            string path = System.IO.Path.Combine(Kit.Tools.Instance.TemporalPath, $"{Guid.NewGuid():N}.jpeg");
            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);

                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    ms.Position = 0;
                    ms.CopyTo(fileStream);
                    return fileStream;
                }
            }
        }

		public FileStream ResizeImage(Stream imageData, float width, float height)
        {
            return ResizeImageAndroid(imageData, width, height);
        }
    }
}
