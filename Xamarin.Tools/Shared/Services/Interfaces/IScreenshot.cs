using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Xamarin.Tools.Shared.Services.Interfaces
{
    public interface IScreenshot
    {
        Task<byte[]> Capture();
    }
}
