using Android.Graphics;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.Views;
using Kit.Services.Interfaces;
using Bitmap = Android.Graphics.Bitmap;

namespace Kit.Droid.Services
{
    public class Screenshot : IScreenshot
    {
        public async Task<byte[]> Capture()
        {
            await Task.Yield();

            View? rootView = CrossCurrentActivity.Current.Activity.Window.DecorView.RootView;
            using (Bitmap? screenshot = Android.Graphics.Bitmap.CreateBitmap(
                rootView.Width,
                rootView.Height,
                Android.Graphics.Bitmap.Config.Argb8888))
            {
                Canvas canvas = new Canvas(screenshot);
                rootView.Draw(canvas);

                using (MemoryStream stream = new MemoryStream())
                {
                    screenshot.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 90, stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
