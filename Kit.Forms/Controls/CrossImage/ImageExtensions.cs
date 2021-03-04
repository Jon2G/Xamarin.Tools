using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Forms.Controls.CrossImage
{
    public class ImageExtensions : Kit.Controls.CrossImage.CrossImageExtensions
    {
        public override Kit.Controls.CrossImage.CrossImage FromFile(FileInfo fileInfo)
        {
            Kit.Forms.Controls.CrossImage.CrossImage image = new Kit.Forms.Controls.CrossImage.CrossImage();
            image.Native = ImageSource.FromFile(fileInfo.FullName);
            return image;
        }
        
        public override Kit.Controls.CrossImage.CrossImage FromStream(Func<Stream> stream)
        {
            Kit.Forms.Controls.CrossImage.CrossImage image = new Kit.Forms.Controls.CrossImage.CrossImage();
            image.Native = ImageSource.FromStream(stream);
            return image;
        }
    }
}
