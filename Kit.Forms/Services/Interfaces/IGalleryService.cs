using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Forms.Services.Interfaces
{
    public interface IGalleryService
    {
        System.Threading.Tasks.Task<string> SaveImageToGallery(Stream stream, string ImageName,string AppName);
    }
}
