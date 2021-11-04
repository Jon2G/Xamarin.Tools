using System.IO;

namespace Kit.Forms.Services.Interfaces
{
    public interface IImageResizer
    {
        public FileStream ResizeImage(Stream imageData, float width, float height);
    }
}
