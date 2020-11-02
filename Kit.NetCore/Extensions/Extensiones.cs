using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.NetCore.Extensions
{
    public static class Extensiones
    {
        public static System.Windows.Media.ImageSource ByteToImage(byte[] imageData)
        {
            if (imageData is null)
            {
                return null;
            }
            System.Windows.Media.Imaging.BitmapImage biImg = new System.Windows.Media.Imaging.BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            System.Windows.Media.ImageSource imgSrc = biImg as System.Windows.Media.ImageSource;

            return imgSrc;
        }
    }
}
