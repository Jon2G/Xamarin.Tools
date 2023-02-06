using Kit.Services.Interfaces;
using System.Threading.Tasks;

namespace Kit.WPF.Services
{
    public class ClipboardService : IClipboardService
    {
        public Task<string> GetText()
        {
            return Task.Run(() => System.Windows.Clipboard.GetText());
        }

        public Task SetText(string text)
        {
            return Task.Run(() => System.Windows.Clipboard.SetText(text));
        }
    }
}
