using FFImageLoading;
using Plugin.Xamarin.Tools.Shared.Classes;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
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

namespace Plugin.Xamarin.Tools.WPF.Services
{
    public class ScreenShotService : IScreenshot
    {
        public async Task<byte[]> Capture()
        {
            await Task.Yield();
            Window w = null;
            RenderTargetBitmap renderTargetBitmap =
                new RenderTargetBitmap(Convert.ToInt32(w.Width), Convert.ToInt32(w.Height), 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(w);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            byte[] buffer = null;
            using (Stream fileStream = new MemoryStream())
            {
                pngImage.Save(fileStream);
                buffer = fileStream.ToByteArray();
            }

            return buffer;
        }

    }
}
