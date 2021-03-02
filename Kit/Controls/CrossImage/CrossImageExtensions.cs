using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kit.Controls.CrossImage
{
   public abstract class CrossImageExtensions
    {
        public abstract Task<CrossImage> FromStream(Func<Stream> stream);

        public abstract CrossImage FromFile(FileInfo fileInfo);

        public async Task<CrossImage> ByteToImage(byte[] imagen)
        {
            return await FromStream(() => new MemoryStream(imagen));
        }
    }
}
