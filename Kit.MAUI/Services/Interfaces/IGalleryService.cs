using System.IO;

namespace Kit.MAUI.Services.Interfaces
{
    public interface IGalleryService
    {
        System.Threading.Tasks.Task<string> SaveImageToGallery(Stream stream, string ImageName,string AppName=null);
    }
}
