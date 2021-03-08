using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Kit.Services.Interfaces;

namespace Tools.UWP.Services
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
