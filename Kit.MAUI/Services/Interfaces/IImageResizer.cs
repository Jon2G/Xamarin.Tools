using System.IO;
using System.Threading.Tasks;

namespace Kit.MAUI.Services.Interfaces
{
    public interface IImageResizer
    {
        public Task<FileStream> ResizeImage(Stream imageData, float width, float height);
    }
}
