using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kit.Services.Interfaces
{
    public interface IQRCode
    {
        MemoryStream Generate(string Value, int Width = 350, int Height = 350, int Margin = 10);
    }
}
