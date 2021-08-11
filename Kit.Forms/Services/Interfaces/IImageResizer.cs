using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Forms.Services.Interfaces
{
    public interface IImageResizer
    {
        public FileStream ResizeImage(Stream imageData, float width, float height);
    }
}
