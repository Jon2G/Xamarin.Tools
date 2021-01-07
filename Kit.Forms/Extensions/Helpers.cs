using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Extensions
{
    public static class Helpers
    {
        public static ImageSource ByteToImage(this byte[] ByteArray)
        {
            return ImageSource.FromStream(() => new MemoryStream(ByteArray));
        }
        public static async Task<byte[]> GetByteArray(StreamImageSource streamImageSource)
        {
            var stream = await streamImageSource.Stream.Invoke(System.Threading.CancellationToken.None);
            return stream.GetByteArray();
        }
        public static byte[] ImageToByte(this ImageSource ImageSource)
        {
            StreamImageSource streamImageSource = (StreamImageSource)ImageSource;
            System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
            Task<Stream> task = streamImageSource.Stream(cancellationToken);
            MemoryStream stream = task.Result as MemoryStream;
            return stream.ToArray();
        }

    }
}
