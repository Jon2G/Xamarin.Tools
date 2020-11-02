using Foundation;
using Kit.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Kit.iOS.Services
{
    public class Screenshot : IScreenshot
    {
        public async Task<byte[]> Capture()
        {
            await Task.Yield();
            var capture = UIScreen.MainScreen.Capture();
            using (NSData data = capture.AsPNG())
            {
                var bytes = new byte[data.Length];
                Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
                return bytes;
            }
        }
    }
}
