extern alias SharedZXingNet;
using Kit.Services.Interfaces;
using Kit.Sql;
using System;
using System.IO;
using Kit.Services.BarCode;
using UIKit;
using Xamarin.Forms;
using SharedZXingNet::ZXing;
using SharedZXingNet::ZXing.Rendering;
using ZXing.Mobile;
using EncodingOptions = SharedZXingNet::ZXing.Common.EncodingOptions;

[assembly: Dependency(typeof(Kit.iOS.Services.BarCodeBuilder))]
namespace Kit.iOS.Services
{
    public class BarCodeBuilder : IBarCodeBuilder
    {
        public MemoryStream Generate(BarcodeFormat Formato, string Value, int Width = 350, int Height = 350, int Margin = 10, EncodingOptions Options = null)
        {
            try
            {
                var barcodeWriter = new SharedZXingNet::ZXing.BarcodeWriter<UIImage>
                {
                    Format = Formato,
                    Options = Options ?? new EncodingOptions { Width = Width, Height = Height, Margin = Margin },
                    Renderer = (IBarcodeRenderer<UIImage>)new BitmapRenderer()
                };

                UIImage bitmap = barcodeWriter.Write(Value);
                MemoryStream stream = (MemoryStream)bitmap.AsPNG().AsStream();
                return stream;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al generar un código QR");
                return null;
            }
        }
    }
}

