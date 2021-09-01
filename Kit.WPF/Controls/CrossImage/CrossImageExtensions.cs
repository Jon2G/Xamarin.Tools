using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kit.WPF.Controls.CrossImage
{
    public class CrossImageExtensions : Kit.Controls.CrossImage.CrossImageExtensions
    {
        public override Kit.Controls.CrossImage.CrossImage FromFile(FileInfo fileInfo)
        {
            Kit.WPF.Controls.CrossImage.CrossImage image = new Kit.WPF.Controls.CrossImage.CrossImage();
            image.Native = new BitmapImage(new Uri(fileInfo.FullName));
            return image;
        }

        public override Kit.Controls.CrossImage.CrossImage FromStream(Func<Stream> stream)
        {
            Kit.WPF.Controls.CrossImage.CrossImage image = new Kit.WPF.Controls.CrossImage.CrossImage();
            //System.Windows.Controls.Image wimage = new System.Windows.Controls.Image();
            using (MemoryStream mstream = (MemoryStream)stream.Invoke())
            {
                if (mstream != null)
                    image.Native = BitmapFrame.Create(mstream,
                                                      BitmapCreateOptions.None,
                                                      BitmapCacheOption.OnLoad);

                //wimage.Source = (BitmapFrame)image.Native;
            }
            //Window w = new Window();
            //w.Content = wimage;
            //w.ShowDialog();
            return image;
        }

        public override Task<byte[]> GetByteArray(Kit.Controls.CrossImage.CrossImage CrossImage)
        {
            throw new NotImplementedException();
        }
    }
}