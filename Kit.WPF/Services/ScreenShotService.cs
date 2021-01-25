
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kit.Services.Interfaces;
using static Kit.Extensions.Helpers;
namespace Kit.WPF.Services
{
    public class ScreenShotService : IScreenshot
    {

        public async Task<byte[]> Capture()
        {
            await Task.Yield();
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            using (Bitmap bmp = new Bitmap((int)screenWidth,
                (int)screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //Opacity = .0;
                    g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);

                    ImageConverter converter = new ImageConverter();
                    return (byte[])converter.ConvertTo(bmp, typeof(byte[]));
                    //Opacity = 1;
                }
            }
        }
        public async Task<byte[]> Capture(Window Window)
        {
            await Task.Yield();
            if (Window is null)
            {
                Window = Application.Current.MainWindow;
            }
            RenderTargetBitmap renderTargetBitmap =
                new RenderTargetBitmap(Convert.ToInt32(Window.Width), Convert.ToInt32(Window.Height), 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(Window);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            byte[] buffer = null;
            using (Stream fileStream = new MemoryStream())
            {
                pngImage.Save(fileStream);
                buffer = Kit.Extensions.Helpers.GetByteArray(fileStream);
            }

            return buffer;
        }

    }
}
