using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Kit.NetCore.Controls.CrossImage
{
    public class ImageExtensions : Kit.Controls.CrossImage.CrossImageExtensions
    {
        public override Kit.Controls.CrossImage.CrossImage FromFile(FileInfo fileInfo)
        {
            NetCore.Controls.CrossImage.CrossImage image = new NetCore.Controls.CrossImage.CrossImage();
            image.Native = new BitmapImage(new Uri(fileInfo.FullName));
            return image;
        }

        public override Kit.Controls.CrossImage.CrossImage FromStream(Func<Stream> stream)
        {
            NetCore.Controls.CrossImage.CrossImage image = new NetCore.Controls.CrossImage.CrossImage();
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
    }
}
