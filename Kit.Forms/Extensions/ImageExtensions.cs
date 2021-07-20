using System.IO;
using System.Threading.Tasks;
using Kit;
using Xamarin.Forms;

namespace Kit.Forms.Extensions
{
    public static partial class ImageExtensions
    {
        public static ImageSource ByteToImage(this byte[] ByteArray)
        {
            return ImageSource.FromStream(() => new MemoryStream(ByteArray));
        }

        public static async Task<byte[]> GetByteArray(StreamImageSource streamImageSource)
        {
            Stream stream = await streamImageSource.Stream.Invoke(System.Threading.CancellationToken.None);
            return await stream.GetByteArray();
        }

        public static byte[] ImageToByte(this ImageSource ImageSource)
        {
            StreamImageSource streamImageSource = (StreamImageSource)ImageSource;
            System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
            Task<Stream> task = streamImageSource.Stream(cancellationToken);
            MemoryStream stream = task.Result as MemoryStream;
            return stream.ToArray();
        }

        public static Stream ImageToStream(this ImageSource ImageSource)
        {
            Stream stream = null;
            switch (ImageSource)
            {
                case StreamImageSource streamImageSource:
                    System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
                    Task<Stream> task = streamImageSource.Stream(cancellationToken);
                    stream = task.Result as MemoryStream;
                    break;

                case FileImageSource fileImageSource:
                    stream = new FileStream(fileImageSource.File, FileMode.Open, FileAccess.Read);
                    break;
            }
            return stream;
        }
    }
}