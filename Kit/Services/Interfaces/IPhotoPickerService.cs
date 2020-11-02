using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Services.Interfaces
{
    public interface IPhotoPickerService
    {
        Task<Tuple<byte[], ImageSource>> GetImageAsync();
    }
}
