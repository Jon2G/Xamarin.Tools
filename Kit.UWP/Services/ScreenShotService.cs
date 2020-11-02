
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;
using Windows.Graphics.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Kit.UWP.Services
{
    public class ScreenShotService : IScreenshot
    {

        public ScreenShotService()
        {

        }

        public async Task<byte[]> Capture()
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(Window.Current.Content);
            IBuffer buffer = await renderTargetBitmap.GetPixelsAsync();
            return buffer.ToArray();
        }
    }
}
