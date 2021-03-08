using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Forms.Services.Interfaces
{
    public interface IPhotoPickerService
    {
        Task<Tuple<byte[], ImageSource>> GetImageAsync();
    }
}
