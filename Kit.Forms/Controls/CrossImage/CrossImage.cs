using Kit.Controls.CrossImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
