using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Services.Interfaces
{
    public interface IClipboardService
    {
        Task SetText(string text);
        Task<string> GetText();
    }
}
