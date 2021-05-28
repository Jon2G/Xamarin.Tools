using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Kit.NetCore.Extensions;

namespace Kit.NetCore.Controls.CrossImage
{
    public class CrossImage : Kit.Controls.CrossImage.CrossImage
    {
        public override byte[] ToArray()
        {
            if (Native is ImageSource wimage)
            {
                return wimage.ImageToBytes();
            }
            return null;
        }

        public override async Task<Stream> ToStream()
        {
            return await Task.FromResult(new MemoryStream(ToArray()));
        }
    }
}