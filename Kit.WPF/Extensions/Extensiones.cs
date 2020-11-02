using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kit.WPF.Extensions
{
    public static class Extensiones
    {
        public static System.Windows.Media.ImageSource ByteToImage(this byte[] imageData)
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
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public static ImageSource ImageSourceFromBitmap(this Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}
