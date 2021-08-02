using Kit.Controls.CrossImage;
using Kit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Kit.WPF.Controls.CrossImage
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