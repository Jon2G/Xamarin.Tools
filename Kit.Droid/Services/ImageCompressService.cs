using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Kit.Droid.Services;
using Kit.Forms.Services.Interfaces;
using Xamarin.Forms;
[assembly: Dependency(typeof(ImageCompressService))]
namespace Kit.Droid.Services
{
    public class ImageCompressService : IImageCompressService
    {
        public async Task<FileStream> CompressImage(Stream imageData, int Quality)
        {
            string path = System.IO.Path.Combine(Kit.Tools.Instance.TemporalPath, $"{Guid.NewGuid():N}.jpeg");
            FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
            Bitmap bitmap = await BitmapFactory.DecodeStreamAsync(imageData);
            if (bitmap is null)
            {
                return null;
            }
            await bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, Quality, stream);
            stream.Flush();
            return stream;
        }
    }
}
