using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Services
{
    public interface IPhotoPickerService
    {
        Task<Tuple<byte[], ImageSource>> GetImageAsync();
    }
}
