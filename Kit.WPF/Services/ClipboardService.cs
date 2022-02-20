using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;

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
