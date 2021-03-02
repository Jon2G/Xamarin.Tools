using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kit.WPF.Controls.CrossImage
{
    public class ImageExtensions : Kit.Controls.CrossImage.CrossImageExtensions
    {
        public override Kit.Controls.CrossImage.CrossImage FromFile(FileInfo fileInfo)
        {
            Kit.WPF.Controls.CrossImage.CrossImage image = new Kit.WPF.Controls.CrossImage.CrossImage();
            image.Native = new BitmapImage(new Uri(fileInfo.FullName));
            return image;
        }
        public override async Task<Kit.Controls.CrossImage.CrossImage> FromStream(Func<Stream> stream)
        {
            await Task.Yield();
            Kit.WPF.Controls.CrossImage.CrossImage image = new Kit.WPF.Controls.CrossImage.CrossImage();
            using (Stream memoryStream = stream.Invoke())
            {
                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = memoryStream;
                imageSource.EndInit();
                // Assign the Source property of your image
                image.Native = imageSource;
            }
            return image;
        }
    }
}
