using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FFImageLoading.Extensions;
using Foundation;
using Kit.iOS.Services;
using Kit.Forms.Services.Interfaces;
using UIKit;
using Xamarin.Forms;
[assembly: Dependency(typeof(ImageCompressService))]

namespace Kit.iOS.Services
{
    [Sql.Attributes.Preserve]
    public class ImageCompressService : IImageCompressService
    {
        public async Task<FileStream> CompressImage(Stream imageData, int Quality)
        {
            await Task.Yield();
            string path = System.IO.Path.Combine(Kit.Tools.Instance.TemporalPath, $"{Guid.NewGuid():N}.jpeg");
            using (NSData nsData = NSData.FromStream(imageData))
            {
                using (UIImage uiimage = UIImage.LoadFromData(nsData))
                {
                    using (Stream compressed = uiimage.AsJpegStream(Quality))
                    {
                        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
                        await compressed.CopyToAsync(stream);
                        return stream;
                    }
                }
            }
        }
    }
}
