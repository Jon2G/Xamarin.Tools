using Kit.Services.Interfaces;
using Kit.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kit.iOS.Services;
using Kit.Services.BarCode;
using UIKit;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing;
using ZXing.Common;

[assembly: Dependency(typeof(Kit.iOS.Services.BarCodeBuilder))]
namespace Kit.iOS.Services
{
    public class BarCodeBuilder : IBarCodeBuilder
    {

        public MemoryStream Generate(BarcodeFormat Formato, string Value, int Width = 350, int Height = 350, int Margin = 10, EncodingOptions Options = null)
        {
            try
            {
                BarcodeWriter barcodeWriter = new ZXing.Mobile.BarcodeWriter
                {
                    Format = Formato,
                    Options = Options ?? new EncodingOptions
                    {
                        Width = Width,
                        Height = Height,
                        Margin = Margin
                    }
                };

                barcodeWriter.Renderer = new ZXing.Mobile.BitmapRenderer();
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

