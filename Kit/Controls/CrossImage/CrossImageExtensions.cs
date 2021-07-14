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
        public abstract CrossImage FromStream(Func<Stream> stream);

        public CrossImage FromFile(string Path) => FromFile(new FileInfo(Path));

        public abstract CrossImage FromFile(FileInfo fileInfo);

        public abstract Task<byte[]> GetByteArray(CrossImage CrossImage);

        public virtual CrossImage FromByteArray(byte[] imagen)
        {
            return FromStream(() => new MemoryStream(imagen));
        }
    }
}