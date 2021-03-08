using System.IO;
using ZXing.Common;

namespace Kit.Services.BarCode
{
    public interface IBarCodeBuilder
    {
        MemoryStream Generate(ZXing.BarcodeFormat Formato, string Value, int Width = 350,
            int Height = 350, int Margin = 10, EncodingOptions Options=null);
    }
}
