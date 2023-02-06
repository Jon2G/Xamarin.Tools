extern alias SharedZXingNet;
using SharedZXingNet::ZXing;
using EncodingOptions = SharedZXingNet::ZXing.Common.EncodingOptions;

namespace Kit.Services.BarCode
{
    public interface IBarCodeBuilder
    {
        MemoryStream Generate(BarcodeFormat Formato, string Value, int Width = 350,
            int Height = 350, int Margin = 10, EncodingOptions Options = null);
    }
}
