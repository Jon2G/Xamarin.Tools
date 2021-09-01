using System.Threading.Tasks;

namespace Kit.Services.Interfaces
{
    public interface IScreenshot
    {
        Task<byte[]> Capture();
    }
}
