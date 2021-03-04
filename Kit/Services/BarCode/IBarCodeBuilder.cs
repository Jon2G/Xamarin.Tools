using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZXing.Common;

namespace Kit.Services.Interfaces
{
    public interface IBarCodeBuilder
    {
        MemoryStream Generate(ZXing.BarcodeFormat Formato, string Value, int Width = 350,
            int Height = 350, int Margin = 10, EncodingOptions Options=null);
    }
}
