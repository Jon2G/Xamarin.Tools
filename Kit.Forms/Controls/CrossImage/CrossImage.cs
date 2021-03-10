using Kit.Controls.CrossImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Forms.Controls.CrossImage
{
    public class CrossImage : Kit.Controls.CrossImage.CrossImage
    {
        public override byte[] ToArray()
        {
            if (Native is ImageSource ximage)
            {
                return Extensions.Helpers.ImageToByte(ximage);
            }
            return null;
        }

        public override async Task<Stream> ToStream()
        {
            if (Native is StreamImageSource simage)
            {
                return await simage.Stream.Invoke(CancellationToken.None);
            }
            else if (Native is ImageSource ximage)
            {
                Stream stream = (Stream)new MemoryStream(this.ToArray());
                return await Task.FromResult(stream);
            }
            return await Task.FromResult<Stream>(null);
        }
    }
}
